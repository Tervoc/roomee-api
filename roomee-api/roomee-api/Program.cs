/*
 * Author(s): Padgett, Matt matthew.padgett@ttu.edu
 * Date Created: February 15 2021
 * Notes: N/A
*/
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace roomee_api
{
    public class Program {
        public static void Main(string[] args) {
            var host = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, builder) => {
                    builder.AddJsonFile("appsettings.json");
                }).ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                })
                .Build();

            host.Run();
            
        }       
    }
}
