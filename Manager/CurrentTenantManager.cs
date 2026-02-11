using Infrastructure;
using Infrastructure.Services;

namespace Services
{
    public class CurrentTenantManager : ITenantGetter, ITenantSetter
    {
        public string Tenant { get; private set; } = ConstantTenants.Demo;
        public void SetTenant(string tenant)
        {
            Tenant = tenant;
        }
    }
}
