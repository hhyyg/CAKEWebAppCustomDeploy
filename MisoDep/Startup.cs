using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MisoDep.Startup))]
namespace MisoDep
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
