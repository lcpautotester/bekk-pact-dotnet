using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Reflection;
using Bekk.Pact.Provider.Web.Config;
using Bekk.Pact.Provider.Web.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Bekk.Pact.Provider.Web.Setup
{
    public abstract class ProviderStateSetupBase : IProviderStateSetup
    {
        public virtual Action<IServiceCollection> ConfigureServices(string providerState)
        {
            var allMethods = GetMethods(providerState);
            var callBacks = allMethods
                .Where(IsServiceCallbackMethod)
                .Select(method => method.Invoke(this, new object[]{})).Cast<Action<IServiceCollection>>();
            var methods = allMethods
                .Where(IsServiceMethod)
                .Select<MethodInfo, Action<IServiceCollection>>(method => svc=> method.Invoke(this, new object[]{svc})).Cast<Action<IServiceCollection>>();
            return svc => {
                try
                {
                    foreach(var callBack in callBacks.Union(methods)) callBack(svc);
                }
                catch(TargetParameterCountException e)
                {
                    throw new SetupException($"An error occured when trying to call setup method for provider state [{providerState}]. A method has an invalid signature.", e);
                }
            };
        }
        public virtual IEnumerable<Claim> GetClaims(string providerState) =>
             GetMethods(providerState).Where(IsClaimsMethod).SelectMany(m => (IEnumerable<Claim>) m.Invoke(this,new object[]{} ));

        private IEnumerable<MethodInfo> GetMethods(string key) => 
            GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(m => m.GetCustomAttributes<ProviderStateAttribute>().Any(a => a.State == key) || m.Name == key);
        private bool IsServiceCallbackMethod(MethodInfo method) => 
            method.ReturnParameter.ParameterType == typeof(Action<IServiceCollection>) &&
            ! method.GetParameters().Any();
        private bool IsServiceMethod(MethodInfo method) =>
            method.ReturnType == typeof(void) &&
            method.GetParameters().Length == 1 &&
            method.GetParameters()[0].ParameterType == typeof(IServiceCollection);
        private bool IsClaimsMethod(MethodInfo method) => 
            method.ReturnType == typeof(IEnumerable<Claim>) &&
            ! method.GetParameters().Any();
    }
}