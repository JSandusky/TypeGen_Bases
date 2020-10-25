using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Trait = System.Collections.Generic.KeyValuePair<string, string>;

namespace typegen
{
    [Flags]
    public enum AccessModifiers
    {
        AM_Default = 0,
        AM_Public = 1,
        AM_Protected = 1 << 1,
        AM_Private = 1 << 2,
        AM_Internal = 1 << 3,
        AM_Abstract = 1 << 4,
        AM_Virtual = 1 << 5,
        AM_Override = 1 << 6,
        AM_Const = 1 << 7,
        AM_Pointer = 1 << 8,        // int*, not preferred
        AM_Reference = 1 << 9,      // int&, not preferred
        AM_Template = 1 << 10,      // it's a template type
        AM_Final = 1 << 11,         // it's tailed with a premier
        AM_Transient = 1 << 12,     // Special #define transient
        AM_Premiere = 1 << 13,      // Special #define premiere
        AM_Volatile = 1 << 14,
        AM_Mutable = 1 << 15,
        AM_Static = 1 << 16,
    };

    public class CodeScanDB
    {
        public class TemplateParam
        {
            public Property Type;
            public int IntegerValue;

            public bool IsInteger { get { return Type == null; } }
        }

        [DebuggerDisplay("Prop: {GetFullTypeName(true)}")]
        public class Property
        {
            /// Modifiers on the property.
            public AccessModifiers accessModifiers_ = 0;
            /// Name of the property.
            public string propertyName_ = "";
            /// The type for that property.
            public ReflectedType type_;
            /// Where do flags bits come from?
            public ReflectedType enumSource_;
            /// If we're a template property then include the params here.
            public List<TemplateParam> templateParameters_ = new List<TemplateParam>();
            /// Extra binding data for it.
            public List<Trait> bindingData_ = new List<Trait>();
            /// Size of the array if this property is an array, will be 0 if not an array.
            public int arraySize_ = 0;

            public string GetFullTypeName(bool withModifiers)
            {
                StringBuilder sb = new StringBuilder();
                
                if (withModifiers && accessModifiers_.HasFlag(AccessModifiers.AM_Const))
                    sb.Append("const ");

                sb.Append(type_.typeName);
                if (type_.isTemplate && templateParameters_.Count > 0)
                {
                    sb.Append("<");
                    for (int i = 0; i < templateParameters_.Count; ++i)
                    {
                        if (i > 0)
                            sb.Append(", ");
                        sb.Append(templateParameters_[i].IsInteger ? templateParameters_[i].IntegerValue.ToString() : templateParameters_[i].Type.GetFullTypeName(withModifiers));
                    }
                    sb.Append(">");
                }

                if (withModifiers && accessModifiers_.HasFlag(AccessModifiers.AM_Pointer))
                    sb.Append("*");
                if (withModifiers && accessModifiers_.HasFlag(AccessModifiers.AM_Reference))
                    sb.Append("&");

                return sb.ToString();
            }

            public bool SameSignature(Property rhs)
            {
                if (accessModifiers_ != rhs.accessModifiers_)
                    return false;
                if (type_ != rhs.type_)
                    return false;
                if (templateParameters_.Count != rhs.templateParameters_.Count)
                    return false;

                for (int i = 0; i < templateParameters_.Count; ++i)
                {
                    if (templateParameters_[i] != rhs.templateParameters_[i])
                        return false;
                }
                return true;
            }

            public bool IsTemplate { get { return type_.isTemplate && templateParameters_.Count > 0; } }

            public bool IsList { 
                get {
                    if (type_.typeName == "std::array" || type_.typeName == "std::vector" || type_.typeName == "ResourceRefList")
                        return true;
                    return false;
                } 
            }

            public bool IsTable {  get {  return type_.typeName == "std::map"; } }

            public bool IsResource { get { return type_.typeName == "ResourceRef" || type_.typeName == "ResourceRefList"; } }
        };

        [DebuggerDisplay("Method: {methodName_}")]
        public class Method
        {
            /// The type containing this method.
            public ReflectedType declaringType_ = null;
            /// Return type.
            public Property returnType_;
            /// Name of the function to call.
            public string methodName_;
            /// List of all of the argument types.
            public List<Property> argumentTypes_ = new List<Property>();
            /// Verbatim copies of the default args. Must be variant convertible.
            public List<string> defaultArguments_ = new List<string>();
            /// Names of the arguments.
            public List<string> argumentNames_ = new List<string>();
            /// Extra binding data for it.
            public List<Trait> bindingData_ = new List<Trait>();
            /// Reason this method was added.
            public SortedSet<string> reason_ = new SortedSet<string>();
            /// Function access modifiers.
            public AccessModifiers accessModifiers_ = 0;

            public bool SameSignature(Method rhs)
            {
                if (!returnType_.SameSignature(rhs.returnType_))
                    return false;

                if (argumentTypes_.Count != rhs.argumentTypes_.Count)
                    return false;

                if (accessModifiers_.HasFlag(AccessModifiers.AM_Const) != accessModifiers_.HasFlag(AccessModifiers.AM_Const))
                    return false;

                for (int i = 0; i < argumentTypes_.Count; ++i)
                {
                    if (!argumentTypes_[i].SameSignature(rhs.argumentTypes_[i]))
                        return false;
                }
                return true;
            }

            public string ReturnTypeText()
            {
                return returnType_.GetFullTypeName(true);
            }

            public string CallSig()
            {
                string r = "(";

                for (int i = 0; i < argumentTypes_.Count; ++i)
                {
                    if (i > 0)
                        r += ", ";
                    r += argumentTypes_[i].GetFullTypeName(true);
                }

                r += ")";
                if (accessModifiers_.HasFlag(AccessModifiers.AM_Const))
                    r += " const";
                return r;
            }
        };

        [DebuggerDisplay("ReflectedType: {typeName}")]
        public class ReflectedType
        {
            /// Has the type been completely resolved?
            public bool isComplete = true;
            /// Is this a basic primitive type?
            public bool isPrimitive = false;
            /// Is this type internal to code, ie. std::string or float.
            public bool isInternal = false;
            /// Is this an array?
            public bool isArray = false;
            /// Is this a template type?
            public bool isTemplate = false;
            /// Is this an abstract base type?
            public bool isAbstract = false;
            /// Is this an final type?
            public bool isFinal = false;
            public bool isVector = false;
            /// If this is an array how long is it?
            public int arrayLength = -1;
            /// Includes template factors
            public string typeName;
            /// Mapped as <access_modifiers, Type>
            public List<ReflectedType> baseClass_ = new List<ReflectedType>();
            /// Assigned state object type
            public ReflectedType stateType_;
            /// If not null then we're a template type.
            public ReflectedType templateParameterType_;
            /// List of the members of the type.
            public List<Property> properties_ = new List<Property>();
            /// List of the bound methods in the type.
            public List<Method> methods_ = new List<Method>();
            /// Extra binding data for it.
            public List<Trait> bindingData_ = new List<Trait>();
            /// If not empty then the type is an enum
            public List<KeyValuePair<string, int>> enumValues_ = new List<KeyValuePair<string, int>>();
            /// VT_??? for the variant type to use for the object, if empty then we assume to treat as VT_VoidPtr
            public string variantType_ = "VT_VoidPtr";
            /// Textual form of the default value.
            public string variantDefault_ = "0x0";

            public List<ReflectedType> derivedTypes_ = new List<ReflectedType>();

            public ReflectedType() { }
            public ReflectedType(string primName) { isPrimitive = true; typeName = primName; }

            public bool IsNumeric
            {
                get
                {
                    switch (typeName)
                    {
                    case "short": return true;
                    case "int": return true;
                    case "uint32_t": return true;

                    case "int8_t": return true;
                    case "uint8_t": return true;
                    case "int16_t": return true;
                    case "uint16_t": return true;
                    case "int64_t": return true;
                    case "uint64_t": return true;

                        case "float": return true;
                    case "double": return true;
                    default: return false;
                    }
                }
            }

            public bool HasMethod(string name)
            {
                return methods_.FirstOrDefault(m => m.methodName_ == name) != null;
            }

            public bool HasAnyFunctions()
            {
                if (methods_.Count > 0)
                    return true;
                var b = this.FirstBase();
                if (b != null)
                    return b.HasAnyFunctions();
                return false;
            }

            public ReflectedType AddProperty(Property prop)
            {
                properties_.Add(prop);
                return this;
            }
        };

        public Dictionary<string, ReflectedType> types_ = new Dictionary<string, ReflectedType>();
        public List<Method> globalFunctions_ = new List<Method>();
        public List<Property> globalProperties_ = new List<Property>();

        ReflectedType GetResolved(ReflectedType cur)
        {
            var found = GetType(cur.typeName);
            if (found == null)
                return cur;
            return found;
        }

        public void ResolveIncompleteTypes()
        {
            foreach (var type in types_)
            {
                var self = type.Value;
                foreach (var property in self.properties_)
                    Resolve(property);
                foreach (var method in self.methods_)
                    Resolve(method);

                for (int i = 0; i < self.baseClass_.Count; ++i)
                { 
                    if (self.baseClass_[i].isComplete == false)
                        self.baseClass_[i] = GetResolved(self.baseClass_[i]);
                }
                
                foreach (var t in type.Value.baseClass_)
                    t.derivedTypes_.Add(self);

                if (type.Value.templateParameterType_ != null && type.Value.templateParameterType_.isComplete == false)
                    type.Value.templateParameterType_ = GetResolved(type.Value.templateParameterType_);
            }

            foreach (var property in globalProperties_)
                Resolve(property);

            foreach (var method in globalFunctions_)
                Resolve(method);
        }

        void Resolve(Property property)
        {
            if (property == null)
                return;

            if (property.type_ != null && !property.type_.isComplete)
            {
                var found = GetResolved(property.type_);
                if (found != null)
                    property.type_ = found;
            }
            
            if (property.type_ != null)
            {
                foreach (var tpl in property.templateParameters_)
                    if (tpl.Type != null && tpl.Type.type_.isComplete == false)
                        tpl.Type.type_ = GetResolved(tpl.Type.type_);
            }
        }

        void Resolve(Method method)
        {
            if (method == null)
                return;

            Resolve(method.returnType_);
            foreach (var arg in method.argumentTypes_)
                Resolve(arg);
        }

        public ReflectedType GetType(string name)
        {
            ReflectedType ret = null;
            types_.TryGetValue(name, out ret);
            return ret;
        }

        public int GetPossibleLiteral(string identifier)
        {
            foreach (var typeInfo in types_)
            {
                if (typeInfo.Value.enumValues_.Count != 0)
                {
                    for (int i = 0; i < typeInfo.Value.enumValues_.Count; ++i)
                    {
                        if (typeInfo.Value.enumValues_[i].Key == identifier)
                            return typeInfo.Value.enumValues_[i].Value;
                    }
                }
            }
            return 0;
        }

        public ReflectedType AddInternalType(string name, string variantType, int code, bool isPrim, bool isTemplate)
        {
            ReflectedType created = new ReflectedType
            {
                typeName = name,
                variantType_ = variantType,
                isPrimitive = isPrim,
                isTemplate = isTemplate,
                isInternal = true,
                isComplete = true
            };
            types_.Add(name, created);
            return created;
        }

        public List<ReflectedType> FlatTypes { get { return types_.Values.ToList(); } }
    }

    public static class CodeScanDB_Ext
    {
        public static CodeScanDB.Method GetBase(this CodeScanDB.Method method)
        {
            if (method.declaringType_ == null)
                return method;

            if (method.accessModifiers_.HasFlag(AccessModifiers.AM_Virtual))
            {
                foreach (var baseType in method.declaringType_.baseClass_)
                {
                    var foundMethod = baseType.methods_.FirstOrDefault(m => m.SameSignature(method) && m.methodName_ == method.methodName_);
                    if (foundMethod != null)
                        return foundMethod;
                }
            }
            
            return method;
        }

        public static CodeScanDB.ReflectedType GetVirtualOriginType(this CodeScanDB.Method method)
        {
            CodeScanDB.Method found = method.GetBase();
            return found.declaringType_;
        }

        public static CodeScanDB.ReflectedType FirstBase(this CodeScanDB.ReflectedType type)
        {
            if (type.baseClass_.Count > 0)
                return type.baseClass_[0];
            return null;
        }

        public static CodeScanDB.ReflectedType GetRoot(this CodeScanDB.ReflectedType type)
        {
            var lastGood = type;
            var found = type.FirstBase();
            do { 
                lastGood = found;
                found = found.FirstBase();
            } while (found != null);
            return lastGood;
        }

        public static int Depth(this CodeScanDB.ReflectedType type)
        {
            int ret = 0;
            var found = type.FirstBase();
            while (found != null)
            {
                ++ret;
                found = found.FirstBase();
            }
            return ret;
        }

        public static List<CodeScanDB.ReflectedType> DepthSortedTypes(this CodeScanDB db)
        {
            return db.types_.Values.OrderBy(o => o.Depth()).ToList();
        }

        public static bool Extends(this CodeScanDB.ReflectedType type, string baseName)
        {
            foreach (var baseT in type.baseClass_)
            { 
                if (baseT.typeName == baseName)
                    return true;
                else if (baseT.Extends(baseName))
                    return true;
            }
            return false;
        }
        
    }
}
