﻿// -----------------------------------------------------------------------
//  <copyright file="Startup.cs" company="OSharp开源团队">
//      Copyright (c) 2014-2015 OSharp. All rights reserved.
//  </copyright>
//  <site>http://www.osharp.org</site>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2015-09-29 23:12</last-date>
// -----------------------------------------------------------------------

using Autofac;
using Hangfire;
using Hangfire.MemoryStorage;

using Microsoft.Owin;

using OSharp.Autofac.Hangfire.Initialize;
using OSharp.Autofac.Http;
using OSharp.Autofac.Mvc;
using OSharp.AutoMapper;
using OSharp.Core.Caching;
using OSharp.Core.Dependency;
using OSharp.Core.Security;
using OSharp.Data.Entity;
using Solution.Web.App;
using Solution.Web.App.Startups.Hangfires;
using OSharp.Logging.Log4Net;
using OSharp.Web.Http.Initialize;
using OSharp.Web.Mvc.Initialize;
using Owin;
using Solution.Core.Services;
using Autofac.Configuration;
using System;

[assembly: OwinStartup(typeof(Startup))]
namespace Solution.Web.App
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // 有关如何配置应用程序的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkID=316888
            ICacheProvider provider = new RuntimeMemoryCacheProvider();
            CacheManager.SetProvider(provider, CacheLevel.First);

            IServicesBuilder builder = new ServicesBuilder();
            IServiceCollection services = builder.Build();
            services.AddLog4NetServices();
            services.AddDataServices();
            services.AddAutoMapperServices();
            services.AddOAuthServices();
            services.AddSolutionServices(app);
            //services.AddCommunicationService();

            IIocBuilder mvcIocBuilder = new MvcAutofacIocBuilder(services);
            app.UseOsharpMvc(mvcIocBuilder);
            IIocBuilder apiIocBuilder = new WebApiAutofacIocBuilder(services);
            app.UseOsharpWebApi(apiIocBuilder);
            //app.UseOsharpSignalR(new SignalRAutofacIocBuilder(services));

            app.ConfigureOAuth(apiIocBuilder.ServiceProvider);
            app.ConfigureWebApi();
            //app.ConfigureSignalR();

            IIocBuilder hangfireBuilder = new HangfireAutofacIocBuilder(services);
            app.UseOSharpHangfile(hangfireBuilder);
            GlobalConfiguration.Configuration.UseMemoryStorage();
            app.UseHangfireDashboard();
            app.UseHangfireServer(new BackgroundJobServerOptions() { WorkerCount = 1 });
            HangfireJobsRunner.Start();

            //启动Communication服务
            //IServiceProvider serviceProvider = apiIocBuilder.Build();
            //ICommunicationContract communicationContracts = serviceProvider.GetService<ICommunicationContract>();
            //if (communicationContracts != null)
            //{
            //    communicationContracts.Initialize();
            //}
        }
    }
}