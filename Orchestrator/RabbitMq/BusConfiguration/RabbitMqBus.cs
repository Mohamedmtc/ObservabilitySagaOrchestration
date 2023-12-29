using Core.RabbitMq.ApiConfiguration;

using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.RabbitMq.BusConfiguration
{
    public static class RabbitMqBus
    {
        public static IBusControl ConfigureBusWebApi(IServiceProvider provider, IConfiguration configuration)
        {
            var RabbitMqSetting = configuration.GetSection(RabbitMqConfiguration.SectionName).Get<RabbitMqConfiguration>();


            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                
                cfg.UseMessageRetry(retry => retry.Interval(5, 1000));
                cfg.Host(new Uri(@"rabbitmq://" + RabbitMqSetting!.HostName + @"/"), hst =>
                {
                    hst.Username(RabbitMqSetting.UserName);
                    hst.Password(RabbitMqSetting.Password);
                });

                cfg.ConfigureEndpoints((IBusRegistrationContext)provider);
            });
        }
    }
}
