﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pavlovLab.Models;
using pavlovLab.Storage;
using Serilog;

namespace pavlovLab
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            ConfigureLogger();

            switch (Configuration["Storage:Type"].ToStorageEnum())
            {
                case StorageEnum.MemCache:
                    services.AddSingleton<IStorage<LabData>, MemCache>();
                    break;
                case StorageEnum.FileStorage:
                    services.AddSingleton<IStorage<LabData>>(
                        x => new FileStorage(Configuration["Storage:FileStorage:Filename"], int.Parse(Configuration["Storage:FileStorage:FlushPeriod"])));
                    break;
                default:
                    throw new IndexOutOfRangeException($"Storage type '{Configuration["Storage:Type"]}' is unknown");
            }

            services.AddScoped<StorageService, StorageService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private void ConfigureLogger()
        {
            var log = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs\\pavlovLab.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Logger = log;
        }
    }
}