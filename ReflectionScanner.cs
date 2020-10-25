using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using STB;

using Trait = System.Collections.Generic.KeyValuePair<string,string>;

/*
    /// Mark a type as reflected, include additional info inside of the var-args as a list
    #define REFLECTED(...)

    /// Mark a global variable to be exposed
    #define REFLECT_GLOBAL(...)
*/

/// Can also tag for a BITFIELD_FLAGS source, an enum with the given name will be used for the bits of an numeric field.
// #define BITFIELD_FLAGS(NAME)

/// Properties are exposed by default
/*
    This can be used to define additional traits for the property:
        name "Pretty Name To Print"
        tip "Textual usage tip"
        depend "FieldName"  (GUI hint: changing this field means having to refresh all fields with "depend")
        Precise (GUI hint: should use 0.01 for steps, instead of 1.0 default)
        Fine    (GUI hint: should use 0.1 for steps, instead of 1.0 default)
        get __GetterMethodName__ (BINDING: getter must be TYPE FUNCTION() const)
        set __SetterMethodName__ (BINDING: setter must be void FUNCTION(const TYPE&) )
        resource __ResourceMember__ (BINDING: named property is the holder for resource data that matches this resource handle object)
    #define PROPERTY(PROPERTY_INFO)
 */

/// Bind a method for GUI exposure
/*
    name "Pretty Name To Print"
    tip "Textual usage tip"
    editor (command will be exposed in the editor GUI, otherwise it is assumed to be for scripting only)
*/
// #define METHOD_COMMAND(METHOD_INFO)
// #define BIND(BINDING_TRAITS)
// #define EVENT_HANDLER()


namespace typegen
{
    /* ReflectionScanner myScanner = new ReflectionScanner();
     * myScanner.Sca(System.IO.File.ReadAllText("MyFile.h"));
     * myScanner.FinishScanning();
     * 
     * var filteredTypes = myScanner.database.FlatTypes.Where(o => !o.isInternal).ToList();
     * foreach (var myType in filteredTypes) {
     *      ... do work ...
     * }
     */
    public class ReflectionScanner
    {
        public CodeScanDB database = new CodeScanDB();
        public SortedSet<string> APIDeclarations = new SortedSet<string>();
        public List<string> ScannedHeaders = new List<string>();
        public List<string> ForwardLines = new List<string>();
        public bool IncludePrivateMembers { get; set; } = false;

        public ReflectionScanner()
        {
            APIDeclarations.Add("DLL_EXPORT");

            // primitives
            database.AddInternalType("void", "void", 0, true, false);
            database.AddInternalType("bool", "bool", 0, true, false);
            database.AddInternalType("int", "int", 0, true, false);
            database.AddInternalType("float", "float", 0, true, false);
            database.AddInternalType("unsigned", "unsigned", 0, true, false);
            database.AddInternalType("uint32_t", "unsigned", 0, true, false);
            database.AddInternalType("double", "double", 0, true, false);
            database.AddInternalType("std::string", "string", 0, true, false);

            // URHO3D
            database.AddInternalType("IntVector2", "IntVector2", 0, true, false)
                .AddProperty(new CodeScanDB.Property { propertyName_ = "x_", type_ = database.GetType("int"), accessModifiers_ = AccessModifiers.AM_Public })
                .AddProperty(new CodeScanDB.Property { propertyName_ = "y_", type_ = database.GetType("int"), accessModifiers_ = AccessModifiers.AM_Public }).isVector = true;
            database.AddInternalType("IntVector3", "IntVector3", 0, true, false)
                .AddProperty(new CodeScanDB.Property { propertyName_ = "x_", type_ = database.GetType("int"), accessModifiers_ = AccessModifiers.AM_Public })
                .AddProperty(new CodeScanDB.Property { propertyName_ = "y_", type_ = database.GetType("int"), accessModifiers_ = AccessModifiers.AM_Public })
                .AddProperty(new CodeScanDB.Property { propertyName_ = "z_", type_ = database.GetType("int"), accessModifiers_ = AccessModifiers.AM_Public }).isVector = true;
            database.AddInternalType("IntRect", "IntRect", 0, true, false)
                .AddProperty(new CodeScanDB.Property { propertyName_ = "left_", type_ = database.GetType("int"), accessModifiers_ = AccessModifiers.AM_Public })
                .AddProperty(new CodeScanDB.Property { propertyName_ = "top_", type_ = database.GetType("int"), accessModifiers_ = AccessModifiers.AM_Public })
                .AddProperty(new CodeScanDB.Property { propertyName_ = "right_", type_ = database.GetType("int"), accessModifiers_ = AccessModifiers.AM_Public })
                .AddProperty(new CodeScanDB.Property { propertyName_ = "bottom_", type_ = database.GetType("int"), accessModifiers_ = AccessModifiers.AM_Public }).isVector = true;
            database.AddInternalType("Rect", "Rect", 0, true, false)
                .AddProperty(new CodeScanDB.Property { propertyName_ = "Left()", type_ = database.GetType("float"), accessModifiers_ = AccessModifiers.AM_Public })
                .AddProperty(new CodeScanDB.Property { propertyName_ = "Top()", type_ = database.GetType("float"), accessModifiers_ = AccessModifiers.AM_Public })
                .AddProperty(new CodeScanDB.Property { propertyName_ = "Right()", type_ = database.GetType("float"), accessModifiers_ = AccessModifiers.AM_Public })
                .AddProperty(new CodeScanDB.Property { propertyName_ = "Bottom()", type_ = database.GetType("float"), accessModifiers_ = AccessModifiers.AM_Public }).isVector = true;
            database.AddInternalType("Vector2", "Vector2", 0, true, false)
                .AddProperty(new CodeScanDB.Property { propertyName_ = "x_", type_ = database.GetType("float"), accessModifiers_ = AccessModifiers.AM_Public })
                .AddProperty(new CodeScanDB.Property { propertyName_ = "y_", type_ = database.GetType("float"), accessModifiers_ = AccessModifiers.AM_Public }).isVector = true;
            database.AddInternalType("Vector3", "Vector3", 0, true, false)
                .AddProperty(new CodeScanDB.Property { propertyName_ = "x_", type_ = database.GetType("float"), accessModifiers_ = AccessModifiers.AM_Public })
                .AddProperty(new CodeScanDB.Property { propertyName_ = "y_", type_ = database.GetType("float"), accessModifiers_ = AccessModifiers.AM_Public })
                .AddProperty(new CodeScanDB.Property { propertyName_ = "z_", type_ = database.GetType("float"), accessModifiers_ = AccessModifiers.AM_Public }).isVector = true;
            database.AddInternalType("Vector4", "Vector4", 0, true, false)
                .AddProperty(new CodeScanDB.Property { propertyName_ = "x_", type_ = database.GetType("float"), accessModifiers_ = AccessModifiers.AM_Public })
                .AddProperty(new CodeScanDB.Property { propertyName_ = "y_", type_ = database.GetType("float"), accessModifiers_ = AccessModifiers.AM_Public })
                .AddProperty(new CodeScanDB.Property { propertyName_ = "z_", type_ = database.GetType("float"), accessModifiers_ = AccessModifiers.AM_Public })
                .AddProperty(new CodeScanDB.Property { propertyName_ = "w_", type_ = database.GetType("float"), accessModifiers_ = AccessModifiers.AM_Public }).isVector = true;
            database.AddInternalType("Quaternion", "Quaternion", 0, true, false)
                .AddProperty(new CodeScanDB.Property { propertyName_ = "x_", type_ = database.GetType("float"), accessModifiers_ = AccessModifiers.AM_Public })
                .AddProperty(new CodeScanDB.Property { propertyName_ = "y_", type_ = database.GetType("float"), accessModifiers_ = AccessModifiers.AM_Public })
                .AddProperty(new CodeScanDB.Property { propertyName_ = "z_", type_ = database.GetType("float"), accessModifiers_ = AccessModifiers.AM_Public })
                .AddProperty(new CodeScanDB.Property { propertyName_ = "w_", type_ = database.GetType("float"), accessModifiers_ = AccessModifiers.AM_Public }).isVector = true;
            database.AddInternalType("SharedPtr", "SharedPtr", 0, false, true);
            database.AddInternalType("Vector", "Vector", 0, false, true);
            database.AddInternalType("PODVector", "PODVector", 0, false, true);
            database.AddInternalType("HashMap", "HashMap", 0, false, true);
            database.AddInternalType("Variant", "Variant", 0, true, false);
            database.AddInternalType("VariantVector", "VariantVector", 0, true, false);
            database.AddInternalType("VariantMap", "VariantMap", 0, true, false);
            database.AddInternalType("Color", "Color", 0, true, false)
                .AddProperty(new CodeScanDB.Property { propertyName_ = "r_", type_ = database.GetType("float"), accessModifiers_ = AccessModifiers.AM_Public })
                .AddProperty(new CodeScanDB.Property { propertyName_ = "g_", type_ = database.GetType("float"), accessModifiers_ = AccessModifiers.AM_Public })
                .AddProperty(new CodeScanDB.Property { propertyName_ = "b_", type_ = database.GetType("float"), accessModifiers_ = AccessModifiers.AM_Public })
                .AddProperty(new CodeScanDB.Property { propertyName_ = "a_", type_ = database.GetType("float"), accessModifiers_ = AccessModifiers.AM_Public }).isVector = true;
            database.AddInternalType("String", "String", 0, true, false);
            database.AddInternalType("StringHash", "StringHash", 0, true, false);

            // MathGeoLib
            database.AddInternalType("float2", "float2", 0, false, false);
            database.AddInternalType("float3", "float3", 0, false, false);
            database.AddInternalType("float4", "float4", 0, false, false);
            database.AddInternalType("rgba", "rgba", 0, false, false);
            database.AddInternalType("Quat", "Quat", 0, false, false);
            database.AddInternalType("float3x3", "float3x3", 0, false, false);
            database.AddInternalType("float3x4", "float3x4", 0, false, false);
            database.AddInternalType("float4x4", "float4x4", 0, false, false);

            // templates
            database.AddInternalType("std::vector", "", 0, false, true);
            database.AddInternalType("std::array", "", 0, false, true);
            database.AddInternalType("std::set", "", 0, false, true);
            database.AddInternalType("std::unordered_map", "", 0, false, true);
            database.AddInternalType("std::map", "", 0, false, true);
        }

        public void Scan(string code)
        {
            code = code.Replace("unsigned char", "uint8_t");
            code = code.Replace("unsigned short", "uint16_t");
            code = code.Replace("unsigned int", "uint32_t");
            code = code.Replace("unsigned long", "uint64_t");
            code = code.Replace("unsigned long long", "uint64_t");
            code = code.Replace("short", "int16_t");
            code = code.Replace("long", "int64_t");

            Lexer lexer = new Lexer(code);
            string minVer = Minimalize(code.Replace("\r", "").Split('\n'));

            List<CodeScanDB.ReflectedType> typeStack = new List<CodeScanDB.ReflectedType>();
            while (AdvanceLexer(lexer) != 0)
            {
                if (lexer.token == Token.ID)
                {
                    if (lexer.string_value == "REFLECTED")
                    {
                        // look for a bound enum or struct
                        ProcessReflected(lexer, database, ref typeStack);
                    }
                    else if (lexer.string_value == "METHOD_CMD")
                    {
                        // global method to be bound, ie. commandlet
                        ReadMember(lexer, null, database, ref typeStack);
                    }
                    else if (lexer.string_value == "REFLECT_GLOBAL")
                    {
                        // Process a global variable, ie CVar like bindings
                        ReadMember(lexer, null, database, ref typeStack);
                    }
                }
            }
        }

        public void ConcludeScanning()
        {
            database.ResolveIncompleteTypes();
        }

        void ProcessReflected(Lexer lexer, CodeScanDB database, ref List<CodeScanDB.ReflectedType> typeStack)
        {
            List<Trait> bindingInfo = new List<Trait>();

            while (AdvanceLexer(lexer) != 0)
            {
                if (lexer.token == Token.ID || lexer.token == Token.DQString)
                {
                    string key = lexer.TokenText;
                    string value = null;
                    if (lexer.Peek() == '=')
                    {
                        AdvanceLexer(lexer);
                        AdvanceLexer(lexer);
                        value = lexer.TokenText;
                    }

                    bindingInfo.Add(new Trait(key, value));
                }
                else if (lexer.token == ')')
                    break;
                else if (lexer.token == '(')
                    continue;
            }

            while (AdvanceLexer(lexer) != 0)
            {
                if (lexer.token == Token.ID)
                {
                    if (lexer.string_value == "struct")
                    {
                        var type = ProcessStruct(lexer, false, database, true, ref typeStack);
                        if (type != null)
                            database.types_.Add(type.typeName, type);
                        type.bindingData_.AddRange(bindingInfo);
                    }
                    else if (lexer.string_value == "class")
                    {
                        var type = ProcessStruct(lexer, true, database, false, ref typeStack);
                        if (type != null)
                            database.types_.Add(type.typeName, type);
                        type.bindingData_.AddRange(bindingInfo);
                    }
                    else if (lexer.string_value == "enum")
                    {
                        var type = ProcessEnum(lexer);
                        if (type != null)
                            database.types_.Add(type.typeName, type);
                        type.bindingData_.AddRange(bindingInfo);
                    }
                }
                return;
            }
        }

        CodeScanDB.ReflectedType ProcessEnum(Lexer lexer)
        {
            string typeName = "";

            if (AdvanceLexer(lexer) != 0 && lexer.token == Token.ID)
                typeName = lexer.TokenText;

            if (string.IsNullOrEmpty(typeName))
                return null;

            CodeScanDB.ReflectedType ret = new CodeScanDB.ReflectedType();
            ret.typeName = typeName;

            while (AdvanceLexer(lexer) != 0 && lexer.token != '{')
                continue;

            int lastEnumValue = 0;
            string valueName = "";

            while (AdvanceLexer(lexer) != 0)
            {
                if (lexer.token == '}')
                    break;

                int value = int.MinValue;
                if (lexer.token == Token.ID && string.IsNullOrEmpty(valueName))
                { 
                    valueName = lexer.string_value;
                    continue;
                }
                else if (lexer.token == '=')
                {
                    while (AdvanceLexer(lexer) != 0)
                    {
                        if (lexer.token == '(')
                            continue;
                        else if (lexer.token == ')')
                            break;
                        else if (lexer.token == ',')
                            break;
                        else if (lexer.token == '}')
                            break;

                        if (lexer.token == Token.IntLit)
                            value = (int)lexer.int_number;
                        else if (lexer.token == Token.ID && lexer.string_value == "FLAG") // FLAG(32)
                        {
                            AdvanceLexer(lexer); //(
                            AdvanceLexer(lexer);
                            if (lexer.token == Token.IntLit)
                                value = 1 << (int)lexer.int_number;
                            else
                                value = database.GetPossibleLiteral(lexer.string_value);
                            AdvanceLexer(lexer); //)
                        }

                        // support 1 << 4
                        if (lexer.token == Token.ShiftLeft)
                        {
                            if (AdvanceLexer(lexer) != 0 && lexer.token == Token.IntLit)
                            {
                                value <<= (int)lexer.int_number;
                                break;
                            }
                        }
                    }
                }

                if (value != int.MinValue && !string.IsNullOrEmpty(valueName))
                {
                    lastEnumValue = value;
                    lastEnumValue += 1;
                    ret.enumValues_.Add(new KeyValuePair<string,int>(valueName, value));
                }
                else if (!string.IsNullOrEmpty(valueName))
                {
                    value = lastEnumValue;
                    lastEnumValue += 1;
                    ret.enumValues_.Add(new KeyValuePair<string, int>(valueName, value));
                }
                valueName = null;
            }

            if (!string.IsNullOrEmpty(valueName))
            {
                ret.enumValues_.Add(new KeyValuePair<string, int>(valueName, lastEnumValue));
            }

            // only return an enum if it contains actual values
            if (ret.enumValues_.Count > 0)
                return ret;

            return null;
        }

        CodeScanDB.ReflectedType ProcessStruct(Lexer lexer, bool isClass, CodeScanDB database, bool defaultIsPublic, ref List<CodeScanDB.ReflectedType> typeStack)
        {
            bool inPublicScope = defaultIsPublic;

            CodeScanDB.ReflectedType type = new CodeScanDB.ReflectedType();
            if (AdvanceLexer(lexer) != 0 && lexer.token == Token.ID)
                type.typeName = lexer.string_value;

            while (AdvanceLexer(lexer) != 0 && (lexer.token != ':' && lexer.token != '{'))
            {
                if (lexer.TokenText == "abtract")
                    type.isAbstract = true;
                if (lexer.TokenText == "final")
                    type.isFinal = true;
                continue;
            }

            if (lexer.token == ':')
            {
                AccessModifiers accessModifiers = 0;
                string baseName = null;
                if (ReadNameOrModifiers(lexer, ref accessModifiers, out baseName))
                {
                    if (!string.IsNullOrEmpty(baseName))
                    {
                        var found = database.GetType(baseName);
                        if (found != null)
                            type.baseClass_.Add(found);
                        else
                        {
                            CodeScanDB.ReflectedType baseType = new CodeScanDB.ReflectedType();
                            type.baseClass_.Add(baseType);
                            baseType.typeName = baseName;
                            baseType.isComplete = false;
                        }
                    }
                }
            }

            while (lexer.token != '}' && AdvanceLexer(lexer) != 0 && lexer.token != '}')
            {
                if (lexer.token == Token.ID && lexer.string_value == "public")
                {
                    AdvanceLexer(lexer);
                    AdvanceLexer(lexer); // eat the trailing :
                    inPublicScope = true;
                }
                else if (lexer.token == Token.ID && lexer.string_value == "private")
                {
                    AdvanceLexer(lexer);
                    AdvanceLexer(lexer); // eat the trailing :
                    inPublicScope = false;
                }

                if (inPublicScope || IncludePrivateMembers)
                    ReadMember(lexer, type, database, ref typeStack);

                while (lexer.token != ';' && lexer.token != Token.EOF)
                    AdvanceLexer(lexer);
            }

            return type;
        }

        static List<CodeScanDB.ReflectedType> FILLER_junkTypes = new List<CodeScanDB.ReflectedType>();

        void ReadMember(Lexer lexer, CodeScanDB.ReflectedType forType, CodeScanDB database, ref List<CodeScanDB.ReflectedType> typeStack)
        {
            CodeScanDB.ReflectedType bitNames = null;

            if (lexer.string_value == "NO_REFLECT")
            {
                while (AdvanceLexer(lexer) != 0 && lexer.token != ';' && lexer.token != '{');
                if (lexer.token == '{')
                    lexer.EatBlock('{', '}');
                return;
            }

            List<Trait> bindingTraits = new List<Trait>();
            bool allowMethodProcessing = false;
            if (lexer.string_value == "PROPERTY" || (allowMethodProcessing = lexer.string_value == "METHOD_CMD") || lexer.string_value == "REFLECT_GLOBAL")
            {
                allowMethodProcessing = true;
                AdvanceLexer(lexer); // eat the ( from METHOD_CMD

                while (AdvanceLexer(lexer) != 0 && (lexer.token == Token.ID || lexer.token == Token.DQString))
                { 
                    string key = lexer.TokenText;
                    string value = null;
                    if (lexer.Peek() == '=')
                    {
                        AdvanceLexer(lexer);
                        AdvanceLexer(lexer);
                        value = lexer.TokenText;
                        if (lexer.Peek() == ':')
                        {
                            value += ":";
                            AdvanceLexer(lexer);
                            AdvanceLexer(lexer);
                            value += lexer.TokenText;
                        }
                    }
                    if (lexer.Peek() == ',')
                        AdvanceLexer(lexer);

                    bindingTraits.Add(new Trait(key, value));
                }

                if (lexer.token == ')')
                    AdvanceLexer(lexer);
            }
            
            if (lexer.string_value == "BITFIELD_FLAGS")
            {
                AdvanceLexer(lexer); // eat the (
                if (AdvanceLexer(lexer) != 0 && lexer.token == Token.ID)
                {
                    var found = database.GetType(lexer.string_value);
                    if (found != null)
                        bitNames = found;
                }
                AdvanceLexer(lexer);
            }

            // We're now at the "int bob;" part
            /*

            cases to handle:
                int data;               // easy case
                int data = 3;           // default initialization ... initializer is grabbed as a string and placed literally in the generated code
                int* data;              // pointers ...   only for function
                int& data;              // references ... only for functions
                const int jim;          // const-ness ... only for functions
                shared_ptr<Roger> bob;  // templates

            special function cases to handle:
                void SimpleFunc();                      // No return and no arguments
                int SimpleFunc();                       // Has a return value
                int SimpleFunc() const;                 // Is a constant method
                void ArgFunc(int argumnet);             // Has an argument
                int ArgFunc(int argumnet = 1);          // Has an argument with default value
                int ArgFunc(const int* argumnet = 0x0); // Has a complex argument
            */

            AccessModifiers mods = 0;
            List<CodeScanDB.TemplateParam> templateParams = new List<CodeScanDB.TemplateParam>();
            string foundName = "";
            var foundType = GetTypeInformation(lexer, database, ref mods, ref templateParams, out foundName);
            if (foundType != null)
            {
                string name = null;
                // if (AdvanceLexer(lexer)) // already there because of GetTypeInformation call
                {
                    if (lexer.token == Token.ID)
                        name = lexer.string_value;
                }

                if (AdvanceLexer(lexer) == 0)
                    return;

                // FUNCTION OR METHOD
                if (lexer.token == '(')
                {
                    // they aren't automatically bound, requiring binding allows making some strictness that wouldn't fly in an autobinding situation
                    // not handling everything under the sun
                    if (allowMethodProcessing)
                    {
                        CodeScanDB.Method newMethod = new CodeScanDB.Method();
                        newMethod.declaringType_ = forType;
                        newMethod.methodName_ = name;
                        newMethod.returnType_ = new CodeScanDB.Property { type_ = foundType, accessModifiers_ = mods & ~AccessModifiers.AM_Virtual };
                        newMethod.bindingData_ = bindingTraits;
                        newMethod.accessModifiers_ = mods & AccessModifiers.AM_Virtual; // only the virtual modifier is allowed at this point
                        if (forType != null)
                            forType.methods_.Add(newMethod);
                        else
                            database.globalFunctions_.Add(newMethod);

                        while (AdvanceLexer(lexer) != 0 && lexer.token != ')' && lexer.token != ';')
                        {
                            // get the argument
                            CodeScanDB.Property prop = new CodeScanDB.Property();
                            newMethod.argumentTypes_.Add(prop);
                            string foundTypeName = "";
                            var functionArgType = GetTypeInformation(lexer, database, ref prop.accessModifiers_, ref prop.templateParameters_, out foundTypeName);
                            if (functionArgType != null)
                                prop.type_ = functionArgType;
                            else
                                prop.type_ = new CodeScanDB.ReflectedType { isComplete = false, typeName = foundTypeName };

                            if (lexer.token == '*')
                            {
                                prop.accessModifiers_ |= AccessModifiers.AM_Pointer;
                                AdvanceLexer(lexer);
                            }
                            else if (lexer.token == '&')
                            {
                                prop.accessModifiers_ |= AccessModifiers.AM_Reference;
                                AdvanceLexer(lexer);
                            }
                            
                            if (lexer.token == Token.ID)
                            {
                                newMethod.argumentNames_.Add(lexer.string_value);
                                AdvanceLexer(lexer);
                            }

                            while (lexer.token != ',' && lexer.token != ')')
                            {
                                if (lexer.token == '=')
                                {
                                    AdvanceLexer(lexer);
                                    newMethod.defaultArguments_.Add(lexer.string_value);
                                }
                                else
                                    AdvanceLexer(lexer);
                            }

                            // make sure we're always the right size, if we've got nothing
                            while (newMethod.defaultArguments_.Count < newMethod.argumentTypes_.Count)
                                newMethod.defaultArguments_.Add(null);
                            while (newMethod.argumentNames_.Count < newMethod.argumentTypes_.Count)
                                newMethod.argumentNames_.Add(null);

                        }

                        if (lexer.PeekText() == "const")
                        {
                            newMethod.accessModifiers_ |= AccessModifiers.AM_Const;
                            AdvanceLexer(lexer);
                        }

                        if (lexer.PeekText() == "override")
                        {
                            newMethod.accessModifiers_ |= AccessModifiers.AM_Override;
                            AdvanceLexer(lexer);
                        }

                        if (lexer.PeekText() == "final")
                        {
                            newMethod.accessModifiers_ |= AccessModifiers.AM_Final;
                            AdvanceLexer(lexer);
                        }

                        if (lexer.PeekText() == "abstract")
                        {
                            newMethod.accessModifiers_ |= AccessModifiers.AM_Abstract;
                            AdvanceLexer(lexer);
                        }

                        if (lexer.Peek() == '{')
                        {
                            lexer.EatBlock('{', '}');
                            lexer.token = ';'; // "inject" the semi-colon
                        }
                    }
                    else
                    {
                        // not processing method, eat until we hit a semi-colon
                        while (AdvanceLexer(lexer) != ')');
                        if (lexer.PeekText() == "const")
                            AdvanceLexer(lexer);
                        if (lexer.PeekText() == "abstract")
                            AdvanceLexer(lexer);
                        if (lexer.Peek() == '{')
                        {
                            lexer.EatBlock('{', '}');
                            lexer.token = ';';
                            return;
                        }
                    }
                }
                // ARRAY, you should be using std::array dumbass!
                else if (lexer.token == '[')
                {
                    CodeScanDB.Property property = new CodeScanDB.Property();
                    property.propertyName_ = name;
                    property.enumSource_ = bitNames;
                    property.type_ = foundType;
                    property.bindingData_ = bindingTraits;
                    if (forType != null)
                        forType.properties_.Add(property);
                    else
                        database.globalProperties_.Add(property);

                    if (AdvanceLexer(lexer) != 0)
                    {
                        if (lexer.token == Token.IntLit)
                            property.arraySize_ = (int)lexer.int_number;
                        else if (lexer.token == Token.ID)
                            property.arraySize_ = database.GetPossibleLiteral(lexer.string_value);
                    }

                    AdvanceLexer(lexer);
                    Debug.Assert(lexer.token == ']');
                }
                // VANILLA FIELD
                else
                {
                    CodeScanDB.Property property = new CodeScanDB.Property();
                    property.propertyName_ = name;
                    property.type_ = foundType;
                    property.enumSource_ = bitNames;
                    property.bindingData_ = bindingTraits;
                    property.templateParameters_ = templateParams;
                    property.accessModifiers_ = mods;
                    if (forType != null)
                        forType.properties_.Add(property);
                    else
                        database.globalProperties_.Add(property);

                    // TODO: extract default value or initialization?

                    while (lexer.token != ';' && lexer.token != Token.EOF)
                        AdvanceLexer(lexer);
                }
            }
            else // unhandled case
            { 
                lexer.EatLine();
                lexer.token = ';';
                //if (lexer.token == '~')
                //    AdvanceLexer(lexer);
                //??Debug.Assert(false, "ReflectionScanner.ReadMember entered unhandled case");
                return;
            }
        }

        CodeScanDB.ReflectedType GetTypeInformation(Lexer lexer, CodeScanDB database, ref AccessModifiers modifiers, ref List<CodeScanDB.TemplateParam> templateParams, out string foundName)
        {
            CodeScanDB.ReflectedType foundType = null;

            foundName = "";
            string name = null;
            if (lexer.token == Token.ID)
            {
                if (lexer.string_value == "static")
                {
                    modifiers |= AccessModifiers.AM_Static;
                    AdvanceLexer(lexer);
                }

                if (lexer.string_value == "virtual")
                {
                    modifiers |= AccessModifiers.AM_Virtual;
                    AdvanceLexer(lexer);
                }

                if (lexer.string_value == "transient")
                {
                    modifiers |= AccessModifiers.AM_Transient;
                    AdvanceLexer(lexer);
                }

                if (lexer.string_value == "const")
                {
                    modifiers |= AccessModifiers.AM_Const;
                    AdvanceLexer(lexer);
                }

                if (lexer.string_value == "mutable")
                {
                    modifiers |= AccessModifiers.AM_Mutable;
                    AdvanceLexer(lexer);
                }

                if (lexer.string_value == "volatile")
                {
                    modifiers |= AccessModifiers.AM_Volatile;
                    AdvanceLexer(lexer);
                }

                // just eat the inline
                if (lexer.string_value == "inline")
                    AdvanceLexer(lexer);

                name = lexer.string_value;
                var found = database.GetType(name);
                foundName = name;
                if (found != null)
                    foundType = found;
            }

            AdvanceLexer(lexer);

            // deal with namespaces
            if (lexer.token == ':')
            {
                AdvanceLexer(lexer);
                if (lexer.token == ':')
                {
                    name += "::";
                    AdvanceLexer(lexer);
                    name += lexer.string_value;
                    AdvanceLexer(lexer);

                    if (foundType == null)
                    {
                        var found = database.GetType(name);
                        if (found != null)
                            foundType = found;
                    }
                }
            }

            if (lexer.token == '<')
            {
                AdvanceLexer(lexer);
                List<CodeScanDB.TemplateParam> templateTypes = new List<CodeScanDB.TemplateParam>();
                do
                {
                    List<CodeScanDB.TemplateParam> junk = new List<CodeScanDB.TemplateParam>();
                    AccessModifiers mods = 0;
                    if (lexer.token == Token.IntLit)
                    {
                        templateTypes.Add(new CodeScanDB.TemplateParam { IntegerValue = (int)lexer.int_number });
                    }
                    else if (lexer.token == Token.ID)
                    {   
                        string foundTemplateName = "";
                        CodeScanDB.ReflectedType templateType = GetTypeInformation(lexer, database, ref mods, ref junk, out foundTemplateName);
                        if (templateType != null)
                            templateTypes.Add(new CodeScanDB.TemplateParam { Type = new CodeScanDB.Property { type_ = templateType, accessModifiers_ = mods, templateParameters_ = junk } });
                        else
                            templateTypes.Add(new CodeScanDB.TemplateParam {  Type = new CodeScanDB.Property { type_ = new CodeScanDB.ReflectedType { isComplete = false, typeName = foundTemplateName }, accessModifiers_ = mods, templateParameters_ = junk } });
                    }
                    if (lexer.Peek() == ',')
                    { 
                        AdvanceLexer(lexer);
                        AdvanceLexer(lexer);
                    }
                    else if (lexer.token == ',')
                        AdvanceLexer(lexer);
                    else
                        AdvanceLexer(lexer);
                } while (lexer.token != '>' && lexer.token != Token.EOF);

                templateParams = templateTypes;
                AdvanceLexer(lexer);
            }

            if (lexer.token == '*')
            {
                modifiers |= AccessModifiers.AM_Pointer;
                AdvanceLexer(lexer);
            }

            if (lexer.token == '&')
            {
                modifiers |= AccessModifiers.AM_Reference;
                AdvanceLexer(lexer);
            }

            return foundType;
        }

        bool ReadNameOrModifiers(Lexer lexer, ref AccessModifiers modifiers, out string name)
        {
            bool ret = false;
            name = null;

            while (AdvanceLexer(lexer) == Token.ID)
            {
                if (lexer.string_value == "public")
                {
                    modifiers |= AccessModifiers.AM_Public;
                    ret = true;
                }
                else if (lexer.string_value == "private")
                {
                    modifiers |= AccessModifiers.AM_Private;
                    ret = true;
                }
                else if (lexer.string_value == "abstract")
                {
                    modifiers |= AccessModifiers.AM_Abstract;
                    ret = true;
                }
                else if (lexer.string_value == "const")
                {
                    modifiers |= AccessModifiers.AM_Const;
                    ret = true;
                }
                else if (lexer.string_value == "virtual")
                {
                    modifiers |= AccessModifiers.AM_Virtual;
                    ret = true;
                }
                else
                {
                    name = lexer.string_value;
                    ret = true;
                }
            }

            return ret;
        }

        int AdvanceLexer(Lexer lexer)
        {
            if (lexer.token == Token.EOF)
                return 0;
            if (lexer.parse_point >= lexer.eof)
                return 0;
            int code = lexer.GetToken();
            if (code != 0)
            {
                // ie. skip URHO3D_API
                if (APIDeclarations.Contains(lexer.TokenText))
                    return AdvanceLexer(lexer);
                //for (var lexCall in lexerCalls)
                //    lexCall(lexer, code);
                return code;
            }
            else
                return 0;
        }

        /// <summary>
        /// Takes the input lines and returns a single block of code with everything deeper than 2 levels removed.
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string Minimalize(string[] source)
        {
            DepthScanner scan = new DepthScanner();
            scan.Process(string.Join("\n", source));
            //+1 namespace
            //+1 type

            List<string> taken = new List<string>();
            for (int i = 0; i < source.Length; ++i)
            {
                if (scan.GetBraceDepth(i) <= 2)
                    taken.Add(source[i]);
            }
            return string.Join("\n", taken);
        }
    }

    public static class BindingExt
    {
        public static bool HasTrait(this List<Trait> traits, string key)
        {
            return traits.Any(t => t.Key == key);
        }

        public static string Get(this List<Trait> bindingTraits, string key, string defaultVal = "")
        {
            for (int i = 0; i < bindingTraits.Count; ++i)
            {
                if (bindingTraits[i].Key == key)
                    return bindingTraits[i].Value;
            }
            return defaultVal;
        }

        public static bool GetBool(this List<Trait> traits, string key, bool defaultVal)
        {
            string v = traits.Get(key).ToLowerInvariant();
            if (!string.IsNullOrEmpty(v))
            {
                if (v == "true" || v == "on")
                    return true;
                else
                    return false;
            }
            return defaultVal;
        }

        public static float GetFloat(this List<Trait> traits, string key, float defaultVal)
        {
            string v = traits.Get(key);
            if (!string.IsNullOrEmpty(v))
            {
                float ret = 0.0f;
                if (float.TryParse(v, out ret))
                    return ret;
            }
            return defaultVal;
        }

        public static int GetInt(this List<Trait> traits, string key, int defaultVal)
        {
            string v = traits.Get(key);
            if (!string.IsNullOrEmpty(v))
            {
                int ret = 0;
                if (int.TryParse(v, out ret))
                    return ret;
            }
            return defaultVal;
        }

        // Double duty, finds repetitive values like:
        //      myVar="some value" myVar = "another Value" 
        // as well as:
        //      myVar = "some value; another value"
        public static List<string> GetList(this List<Trait> traits, string key)
        {
            List<string> ret = new List<string>();
            for (int i = 0; i < traits.Count; ++i)
            {
                if (traits[i].Key == key)
                {
                    var split = traits[i].Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (split != null && split.Count > 0)
                        ret.AddRange(split.ConvertAll(s => s.Trim()));
                }
            }
            return ret;
        }

        public static KeyValuePair<float, float> GetRange(this List<Trait> self, string traitName, KeyValuePair<float, float> defVal)
        {
            foreach (Trait t in self)
            {
                if (t.Key == traitName)
                {
                    var terms = t.Value.Split(':');
                    return new KeyValuePair<float, float>(float.Parse(terms[0]), float.Parse(terms[1]));
                }
            }
            return defVal;
        }

        public static string AngelscriptSignature(this CodeScanDB.Method method)
        {
            return $"{method.ReturnTypeText().Replace("*", "@+")} {method.methodName_}{method.CallSig().Replace("*", "@+")}";
        }
    }
}
