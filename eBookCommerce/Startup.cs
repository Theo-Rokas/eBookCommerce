using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(eBookCommerce.Startup))]
namespace eBookCommerce
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
