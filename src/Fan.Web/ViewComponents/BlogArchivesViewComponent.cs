﻿using Fan.Blog.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Fan.Web.ViewComponents
{
    /// <summary>
    /// The BlogArchives view component.
    /// </summary>
    public class BlogArchivesViewComponent : ViewComponent
    {
        private readonly IStatsService _statsSvc;
        public BlogArchivesViewComponent(IStatsService statsService)
        {
            _statsSvc = statsService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var years = await _statsSvc.GetArchivesAsync();
            return View(years);
        }
    }
}