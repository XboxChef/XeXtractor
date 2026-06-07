using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;


namespace XeXtractor.Properties
{

    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    [CompilerGenerated]
    [DebuggerNonUserCode]
    internal class Resources
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        internal Resources()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager
        {
            get
            {
                if (XeXtractor.Properties.Resources.resourceMan == null)
                    XeXtractor.Properties.Resources.resourceMan = new ResourceManager("XeXtractor.Properties.Resources", typeof(XeXtractor.Properties.Resources).Assembly);
                return XeXtractor.Properties.Resources.resourceMan;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get => XeXtractor.Properties.Resources.resourceCulture;
            set => XeXtractor.Properties.Resources.resourceCulture = value;
        }
    }
}