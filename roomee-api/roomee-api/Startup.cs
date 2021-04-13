/*
 * Author(s): Padgett, Matt matthew.padgett@ttu.edu
 * Date Created: February 15 2021
 * Notes: N/A
*/
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace roomee_api {
	public class Startup {
		public static string ConnectionString { get; private set; }
		public static string JWTSecret { get; private set; }

		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services) {
			services.AddControllers().AddNewtonsoftJson();

			services.AddCors(options => {
				options.AddPolicy("local",
					builder => {
						builder.WithOrigins("http://localhost:3000")
						.AllowAnyHeader()
						.AllowAnyMethod();
					});

				options.AddPolicy("default",
					builder => {
						builder.WithOrigins("http://roomee.mpadgett.net")
						.AllowAnyHeader()
						.AllowAnyMethod();

						builder.WithOrigins("http://localhost:3000")
						.AllowAnyHeader()
						.AllowAnyMethod();
					});
			});

			services.AddMvc();
			services.AddSwaggerGen(c => {
				c.SwaggerDoc("v1", new OpenApiInfo {
					Title = "roomee API",
					Version = "v1",
					Contact = new OpenApiContact { Name = "Christian Parrish", Email = "christian.parrish@ttu.edu" },
					Description = "API for use by roomee clients."
				});
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			ConnectionString = Configuration.GetConnectionString("RoomeeConnStr");
			JWTSecret = Configuration.GetValue<string>("JWTSecret");

			app.UseCors("default");

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseEndpoints(endpoints => {
				endpoints.MapControllers();
			});

			app.UseSwagger();
			app.UseSwaggerUI(c => {
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "roomee API v1");
			});
		}
	}
}
