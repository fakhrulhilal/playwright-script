using Microsoft.Playwright;
using PlaywrightScript;

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.LaunchEdge();
var page = await browser.GetPageAsync();
await page.CleanupWishlist();