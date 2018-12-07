﻿using Fan.Blog.Data;
using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Services;
using Fan.Blog.Services.Interfaces;
using Fan.Data;
using Fan.Medias;
using Fan.Settings;
using Fan.Shortcodes;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading.Tasks;

namespace Fan.Blog.IntegrationTests.Base
{
    /// <summary>
    /// Blog integration test base.
    /// </summary>
    public class BlogServiceIntegrationTestBase : BlogIntegrationTestBase
    {
        protected IBlogPostService _blogSvc;
        protected ICategoryService _catSvc;
        protected ITagService _tagSvc;
        protected IImageService _imgSvc;
        protected Mock<ISettingService> _settingSvcMock;
        protected Mock<IMediaService> _mediaSvcMock;
        protected ILoggerFactory _loggerFactory;
        private readonly IMediaService _mediaSvc;
        protected Mock<IStorageProvider> _storageProviderMock;

        protected const string STORAGE_ENDPOINT = "https://www.fanray.com";

        public BlogServiceIntegrationTestBase()
        {
            // ---------------------------------------------------------------- repos

            var catRepo = new SqlCategoryRepository(_db);
            var tagRepo = new SqlTagRepository(_db);
            var postRepo = new SqlPostRepository(_db);

            // ---------------------------------------------------------------- mock SettingService 

            _settingSvcMock = new Mock<ISettingService>();
            _settingSvcMock.Setup(svc => svc.GetSettingsAsync<CoreSettings>()).Returns(Task.FromResult(new CoreSettings()));
            _settingSvcMock.Setup(svc => svc.GetSettingsAsync<BlogSettings>()).Returns(Task.FromResult(new BlogSettings()));

            // ---------------------------------------------------------------- mock AppSettings

            var appSettingsMock = new Mock<IOptionsSnapshot<AppSettings>>();
            appSettingsMock.Setup(o => o.Value).Returns(new AppSettings());

            // ---------------------------------------------------------------- mock IStorageProvider

            _storageProviderMock = new Mock<IStorageProvider>();
            _storageProviderMock.Setup(pro => pro.StorageEndpoint).Returns(STORAGE_ENDPOINT);

            // ---------------------------------------------------------------- MediaService

            var mediaRepo = new SqlMediaRepository(_db);
            _mediaSvc = new MediaService(_storageProviderMock.Object, appSettingsMock.Object, mediaRepo);

            // ---------------------------------------------------------------- Cache

            var serviceProvider = new ServiceCollection().AddMemoryCache().AddLogging().BuildServiceProvider();
            var memCacheOptions = serviceProvider.GetService<IOptions<MemoryDistributedCacheOptions>>();
            var cache = new MemoryDistributedCache(memCacheOptions);

            // ---------------------------------------------------------------- LoggerFactory

            _loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var loggerBlogSvc = _loggerFactory.CreateLogger<BlogPostService>();
            var loggerCatSvc = _loggerFactory.CreateLogger<CategoryService>();
            var loggerTagSvc = _loggerFactory.CreateLogger<TagService>();

            // ---------------------------------------------------------------- Mapper, Shortcode

            var mapper = BlogUtil.Mapper;
            var shortcodeSvc = new Mock<IShortcodeService>();

            // ---------------------------------------------------------------- MediatR and Services

            var services = new ServiceCollection();
            services.AddScoped<ServiceFactory>(p => p.GetService);  // MediatR.ServiceFactory
            services.AddSingleton<IDistributedCache>(cache);        // cache
            services.AddSingleton<FanDbContext>(_db);               // DbContext for repos
            services.Scan(scan => scan
               .FromAssembliesOf(typeof(IMediator), typeof(ILogger), typeof(IBlogPostService), typeof(ISettingService))
               .AddClasses()
               .AsImplementedInterfaces());

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            _catSvc = new CategoryService(catRepo, _settingSvcMock.Object, mediator, cache, loggerCatSvc);
            _tagSvc = new TagService(tagRepo, mediator, cache, loggerTagSvc);
            _imgSvc = new ImageService(_mediaSvc, _storageProviderMock.Object, appSettingsMock.Object);

            // the blog service
            _blogSvc = new BlogPostService(
                _settingSvcMock.Object, 
                postRepo, 
                cache, 
                loggerBlogSvc, 
                mapper, 
                shortcodeSvc.Object, 
                mediator);
        }
    }
}
