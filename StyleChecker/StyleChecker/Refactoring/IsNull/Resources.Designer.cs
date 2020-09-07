//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StyleChecker.Refactoring.IsNull {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("StyleChecker.Refactoring.IsNull.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use &apos;==&apos; or &apos;!=&apos; operator when a variable is compared to null..
        /// </summary>
        internal static string Description {
            get {
                return ResourceManager.GetString("Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Replace &apos;... is null&apos; with &apos;... == null&apos;..
        /// </summary>
        internal static string FixTitleEqualNull {
            get {
                return ResourceManager.GetString("FixTitleEqualNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Replace &apos;!(... is null)&apos; with &apos;... is {}&apos;..
        /// </summary>
        internal static string FixTitleIsBraces {
            get {
                return ResourceManager.GetString("FixTitleIsBraces", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Replace &apos;!(... is null)&apos; with &apos;... != null&apos;..
        /// </summary>
        internal static string FixTitleNotEqualNull {
            get {
                return ResourceManager.GetString("FixTitleNotEqualNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Replace &apos;... is null&apos; with &apos;!(... is {})&apos;..
        /// </summary>
        internal static string FixTitleNotIsBraces {
            get {
                return ResourceManager.GetString("FixTitleNotIsBraces", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use &apos;{0}&apos; operator instead of &apos;is&apos; pattern matching..
        /// </summary>
        internal static string MessageFormat {
            get {
                return ResourceManager.GetString("MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not use &apos;is&apos; pattern matching with &apos;null&apos;..
        /// </summary>
        internal static string Title {
            get {
                return ResourceManager.GetString("Title", resourceCulture);
            }
        }
    }
}
