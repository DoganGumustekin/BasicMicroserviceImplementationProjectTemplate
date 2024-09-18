using CatalogService.Api.Core.Domain;
using Polly;
using System.Data.SqlClient;
using System.Globalization;
using System.IO.Compression;

namespace CatalogService.Api.Infrastructure.Context
{
    public class CatalogContextSeed
    {
        public async Task SeedAsync(CatalogContext context, IWebHostEnvironment env, ILogger<CatalogContextSeed> logger)
        {
            var policy = Policy.Handle<SqlException>().
                WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                    onRetry: (exception, timeSpan, retry, ctx) =>
                    {
                        logger.LogWarning(exception, "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {retry}");
                    }
                );

            var setupDirPath = Path.Combine(env.ContentRootPath, "Infrastructure", "Setup", "SeedFiles");
            var picturePath = "Pics";

            await policy.ExecuteAsync(() => ProcessSeeding(context, setupDirPath, picturePath, logger));
        }

        private async Task ProcessSeeding(CatalogContext context, string setupDirPath, string picturePath, ILogger logger)
        {
            if (!context.CatalogBrands.Any()) //eğer tabloda hiç kayıt yok ise
            {
                await context.CatalogBrands.AddRangeAsync(GetCatalogBrandsFromFile(setupDirPath)); //dosyadan al getir

                await context.SaveChangesAsync(); //kaydet
            }

            if (!context.CatalogTypes.Any())
            {
                await context.CatalogTypes.AddRangeAsync(GetCatalogTypeFromFile(setupDirPath));

                await context.SaveChangesAsync();
            }

            if (!context.CatalogItems.Any())
            {
                await context.CatalogItems.AddRangeAsync(GetCatalogItemsFromFile(setupDirPath, context));

                await context.SaveChangesAsync();

                GetCatalogItemPictures(setupDirPath, picturePath);
            }
        }

        private IEnumerable<CatalogBrand> GetCatalogBrandsFromFile(string contentPath)
        {
            IEnumerable<CatalogBrand> GetPreConfiguredCatalogBrands()
            {
                return new List<CatalogBrand>()
                {
                    new CatalogBrand() { Brand = "Azure"},
                    new CatalogBrand() { Brand = ".NET"},
                    new CatalogBrand() { Brand = "Visual Studio"},
                    new CatalogBrand() { Brand = "SQL Server"},
                    new CatalogBrand() { Brand = "Other"}
                };
            }

            string fileName = Path.Combine(contentPath, "BrandsTextFile.txt");

            if (!File.Exists(fileName))
            {
                return GetPreConfiguredCatalogBrands();
            }

            var fileContent = File.ReadAllLines(fileName);

            var list = fileContent.Select(i => new CatalogBrand()
            {
                Brand = i.Trim('"')
            }).Where(i => i != null);

            return list ?? GetPreConfiguredCatalogBrands();
        }
        private IEnumerable<CatalogType> GetCatalogTypeFromFile(string contentPath)
        {
            IEnumerable<CatalogType> GetPreConfiguredCatalogTypes()
            {
                return new List<CatalogType>()
                {
                    new CatalogType() { Type = "Mug"},
                    new CatalogType() { Type = "T-Shirt"},
                    new CatalogType() { Type = "Sheet"},
                    new CatalogType() { Type = "USB Memory Stick"}
                };
            }

            string fileName = Path.Combine(contentPath, "CatalogTypes.txt");

            if (!File.Exists(fileName))
            {
                return GetPreConfiguredCatalogTypes();
            }

            var fileContent = File.ReadAllLines(fileName);

            var list = fileContent.Select(i => new CatalogType()
            {
                Type = i.Trim('"')
            }).Where(i => i != null);

            return list ?? GetPreConfiguredCatalogTypes();
        }
        private IEnumerable<CatalogItem> GetCatalogItemsFromFile(string contentPath, CatalogContext context)
        {
            IEnumerable<CatalogItem> GetPreConfiguredCatalogItems()
            {
                return new List<CatalogItem>()
                {
                    new CatalogItem {Name = "Product1", Description = "xxx", AvailableStock = 100, Price = 22, PictureFileName = "image1.jpg", PictureUri = "picture1.png", CatalogTypeId = 1, CatalogBrandId = 2},
                    new CatalogItem {Name = "Product2", Description = "aaa", AvailableStock = 100, Price = 25, PictureFileName = "image2.jpg", PictureUri = "picture2.png", CatalogTypeId = 2, CatalogBrandId = 2},
                    new CatalogItem {Name = "Product3", Description = "ddd", AvailableStock = 100, Price = 225, PictureFileName = "image3.jpg", PictureUri = "picture3.png", CatalogTypeId = 3, CatalogBrandId = 5},
                    new CatalogItem {Name = "Product4", Description = "fff", AvailableStock = 100, Price = 2274, PictureFileName = "image4.jpg", PictureUri = "picture4.png", CatalogTypeId = 1, CatalogBrandId = 2},
                    new CatalogItem {Name = "Product5", Description = "rrr", AvailableStock = 100, Price = 127, PictureFileName = "image5.jpg", PictureUri = "picture5.png", CatalogTypeId = 2, CatalogBrandId = 5},
                    new CatalogItem {Name = "Product6", Description = "www", AvailableStock = 100, Price = 222, PictureFileName = "image6.jpg", PictureUri = "picture6.png", CatalogTypeId = 3, CatalogBrandId = 2},
                    new CatalogItem {Name = "Product7", Description = "qqq", AvailableStock = 100, Price = 2255, PictureFileName = "image7.jpg", PictureUri = "picture7.png", CatalogTypeId = 3, CatalogBrandId = 5},
                    new CatalogItem {Name = "Product8", Description = "ddfdf", AvailableStock = 100, Price = 228, PictureFileName = "image8.jpg", PictureUri = "picture8.png", CatalogTypeId = 1, CatalogBrandId = 5},
                    new CatalogItem {Name = "Product9", Description = "fffdf", AvailableStock = 100, Price = 58, PictureFileName = "image9.jpg", PictureUri = "picture9.png", CatalogTypeId = 1, CatalogBrandId = 5},
                    new CatalogItem {Name = "Product10", Description = "vvv", AvailableStock = 100, Price = 251, PictureFileName = "image10.jpg", PictureUri = "picture10.png", CatalogTypeId = 2, CatalogBrandId = 2},
                    new CatalogItem {Name = "Product11", Description = "cvxv", AvailableStock = 100, Price = 77, PictureFileName = "image11.jpg", PictureUri = "picture11.png", CatalogTypeId = 2, CatalogBrandId = 2},
                    new CatalogItem {Name = "Product12", Description = "ertw", AvailableStock = 100, Price = 99, PictureFileName = "image12.jpg", PictureUri = "picture12.png", CatalogTypeId = 3, CatalogBrandId = 5},
                    new CatalogItem {Name = "Product13", Description = "dasda", AvailableStock = 100, Price = 87, PictureFileName = "image13.jpg", PictureUri = "picture13.png", CatalogTypeId = 1, CatalogBrandId = 5},
                    new CatalogItem {Name = "Product14", Description = "mhgmghm", AvailableStock = 100, Price = 78, PictureFileName = "image14.jpg", PictureUri = "picture14.png", CatalogTypeId = 1, CatalogBrandId = 2}
                };
            }

            string fileName = Path.Combine(contentPath, "CatalogItems.txt");

            if (!File.Exists(fileName))
            {
                return GetPreConfiguredCatalogItems();
            }

            var catalogTypeIdLookUp = context.CatalogTypes.ToDictionary(ct => ct.Type, ct => ct.Id);
            var catalogBrandIdLookUp = context.CatalogBrands.ToDictionary(ct => ct.Brand, ct => ct.Id);

            var fileContent = File.ReadLines(fileName)
                .Skip(1)
                .Select(i => i.Split(','))
                .Select(i => new CatalogItem()
                {
                    CatalogTypeId = catalogTypeIdLookUp[i[0]],
                    CatalogBrandId = catalogTypeIdLookUp[i[1]],
                    Description = i[2].Trim('"').Trim(),
                    Name = i[3].Trim('"').Trim(),
                    Price = Decimal.Parse(i[4].Trim('"').Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
                    PictureFileName = i[5].Trim('"').Trim(),
                    AvailableStock = string.IsNullOrEmpty(i[6]) ? 0 : int.Parse(i[6]),
                    OnReorder = Convert.ToBoolean(i[7])
                });
            return fileContent;
        }

        private void GetCatalogItemPictures(string contentPath, string picturePath)
        {
            picturePath ??= "pics"; //dışarıdan bir path gönderilmezsse otomatik olarak ana klasörde pics isminde bir klasör oluştur.

            if (picturePath != null)
            {
                DirectoryInfo directory = new DirectoryInfo(picturePath);
                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();
                }

                string zipFileCatalogItemPictures = Path.Combine(contentPath, "CatalogItems.zip"); //bunun içindekileri dışardan gelen klasöre çıkart.
                ZipFile.ExtractToDirectory(zipFileCatalogItemPictures, picturePath);
            }
        }
    }
}
