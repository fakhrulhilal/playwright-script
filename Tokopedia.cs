using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Playwright;

namespace PlaywrightScript;

/// <summary>
/// 
/// </summary>
internal static class Tokopedia
{
    private const int PagingSize = 5 * 4;

    /// <summary>
    /// Clean all wishlist
    /// </summary>
    /// <param name="page"></param>
    /// <param name="csvBackupPath">Full path to backup CSV</param>
    public static async Task CleanupWishlist(this IPage page, string? csvBackupPath = null)
    {
        csvBackupPath ??= Path.Combine(Path.GetTempPath(),
            $"tokopedia_wishlist_backup_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        await using var writer = new StreamWriter(csvBackupPath);
        await using var csv = new CsvHelper.CsvWriter(writer, CultureInfo.InvariantCulture);
        await page.GotoAsync("https://www.tokopedia.com/wishlist/all");
        await page.Locator(".sort button[data-unify='Select']").ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Terlama Disimpan" }).ClickAsync();
        csv.WriteHeader<Product>();
        await csv.NextRecordAsync();
        var writtenRecord = 0;
        bool hasNextPage = true;
        while (hasNextPage)
        {
            foreach (var productElement in await page.GetByTestId("master-product-card").AllAsync())
            {
                var productName = await productElement.GetByTestId("linkProductName").TextContentAsync() ??
                                  "<unknown product>";
                var productUrl = await productElement.GetProductUrl();
                var product = new Product(productName, productUrl);
                csv.WriteRecord(product);
                await csv.NextRecordAsync();
                writtenRecord++;
                if (writtenRecord == PagingSize)
                {
                    await csv.FlushAsync();
                    writtenRecord = 0;
                }

                await productElement.GetByRole(AriaRole.Img, new() { Name = "btn-cta-icon" }).ClickAsync();
                await page.Locator("[data-unify='Card']")
                    .GetByRole(AriaRole.Button, new() { Name = "Hapus" }).ClickAsync();
                await page.GetByRole(AriaRole.Dialog)
                    .Filter(new() { HasText = "Hapus 1 barang dari Wishlist" })
                    .GetByRole(AriaRole.Button, new() { Name = "Hapus" })
                    .ClickAsync();
            }

            var nextPage = page.GetByRole(AriaRole.Navigation, new() { Name = "Navigasi laman" })
                .GetByRole(AriaRole.Button, new() { NameString = "Laman 2" });
            hasNextPage = await nextPage.IsVisibleAsync();
        }
    }

    private static async Task<string> GetProductUrl(this ILocator locator)
    {
        var href = await locator.Locator("a[class*='info-content']").GetAttributeAsync("href");
        return string.IsNullOrWhiteSpace(href) ? "<unknown url>" : href.Split('?', 2)[0];
    }
}

[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global", Justification = "Used by CSV")]
readonly record struct Product(string Name, string Url);