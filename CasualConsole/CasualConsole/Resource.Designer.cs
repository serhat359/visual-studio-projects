﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CasualConsole {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CasualConsole.Resource", typeof(Resource).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to asd,24,&quot;hel,lo&quot;,bla bla bla,&quot;yet another example where he said &quot;&quot;hi!&quot;&quot;, can you believe it?&quot;,24.5.
        /// </summary>
        public static string CsvText {
            get {
                return ResourceManager.GetString("CsvText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;rss version=&quot;2.0&quot;&gt;
        ///	&lt;channel&gt;
        ///		&lt;item&gt;
        ///			&lt;title&gt;YGOTAS Episode 75 - Valley of the Duels&lt;/title&gt;
        ///			&lt;link&gt;https://www.youtube.com/watch?v=YSAiv93D7jg&lt;/link&gt;
        ///			&lt;description&gt;It&apos;s a good thing I played all that YouTubers Life. Special thanks to Krosecz for editing this episode! Check out Krosecz&apos;s channel at ...&lt;/description&gt;
        ///			&lt;pubDate&gt;Tue, 20 Jun 2017 15:55:19 GMT&lt;/pubDate&gt;
        ///		&lt;/item&gt;
        ///		&lt;item&gt;
        ///			&lt;title&gt;Episode 74 - Right In The Feels&lt;/title&gt;
        ///			&lt;link&gt;https [rest of string was truncated]&quot;;.
        /// </summary>
        public static string ErrorContainingXml {
            get {
                return ResourceManager.GetString("ErrorContainingXml", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to return ExecuteToList&lt;{1}&gt;(&quot;{0}&quot;, container.ToList());.
        /// </summary>
        public static string NewPattern {
            get {
                return ResourceManager.GetString("NewPattern", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DataTable table = ExecuteToTable\(&quot;(?&lt;spname&gt;\w+)&quot;, container.ToList\(\)\);
        ///
        ///return MapAll&lt;(?&lt;classname&gt;\w+)&gt;\(table\);.
        /// </summary>
        public static string Pattern {
            get {
                return ResourceManager.GetString("Pattern", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to // comment
        ///
        ///DataTable table = ExecuteToTable(&quot;Brand_Select&quot;, container.ToList());
        ///
        ///return MapAll&lt;Brand&gt;(table);
        ///
        ///// comment
        ///
        ///DataTable table = ExecuteToTable(&quot;Other_StoredProcedure&quot;, container.ToList());
        ///
        ///return MapAll&lt;OtherClass&gt;(table);.
        /// </summary>
        public static string Statement1 {
            get {
                return ResourceManager.GetString("Statement1", resourceCulture);
            }
        }
    }
}
