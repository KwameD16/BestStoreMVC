using BestStoreMVC.Models;
using BestStoreMVC.Service;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreMVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context=context;
            this.environment=environment;
        }




        public IActionResult Index()
        {
            var products = context.Products.OrderByDescending(p => p.Id).ToList();

            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductDto productDto)
        {

            if (productDto.ImageFile == null) {

                ModelState.AddModelError("ImageFile", "The image file required");
                return View();
            
            }

            if (!ModelState.IsValid) { 
            
                return View(productDto);
            
            }

            //Save the Image file
            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            newFileName += Path.GetExtension(productDto.ImageFile!.FileName);

            string imageFullPath = environment.WebRootPath + "/Products/" + newFileName;
            using (var stream = System.IO.File.Create(imageFullPath)) { 
            
            productDto.ImageFile.CopyTo(stream);

            }

            // save the new product in the database
            Product product = new Product()
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Category = productDto.Category,
                Price = productDto.Price,
                Description = productDto.Description,
                ImageFileName = newFileName,
                CreatedAt = DateTime.Now,
            };

            context.Products.Add(product);
            context.SaveChanges();
            
            return RedirectToAction("Index","Product");
        }

        public IActionResult Edit(int id) {

            var product = context.Products.Find(id);
            if (product == null) { return RedirectToAction("Index", "Product"); }

            // create productDto from product
            var productDto = new ProductDto()
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description,
            };

            ViewData["ProductId"] =product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["Brandd"] = product.CreatedAt.ToString("MM/dd/yyyy");

            return View(productDto);
        
        
        }

        [HttpPost]
        public IActionResult Edit(int id, ProductDto productDto)
        {
            var product = context.Products.Find(id);

            if (product == null)
            {

                return RedirectToAction("Index", "Product");

            }

            if (!ModelState.IsValid)
            {
                ViewData["ProductId"] =product.Id;
                ViewData["ImageFileName"] = product.ImageFileName;
                ViewData["Created"] = product.CreatedAt.ToString("MM/dd/yyyy");
                return View(productDto);
            }

            //Update the image filr if we have a new image
            string newFileName = product.ImageFileName;
            if (productDto.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                newFileName += Path.GetExtension(productDto.ImageFile.FileName);

                string imageFullPath = environment.WebRootPath + "/Products/" + newFileName;
                using (var stream = System.IO.File.Create(imageFullPath))
                {

                    productDto.ImageFile.CopyTo(stream);

                }

                //delete the old image
                string oldImageFullPath = environment.WebRootPath + "/Products/" + product.ImageFileName;
                System.IO.File.Delete(oldImageFullPath);
            }


            // updaate the product in the database
            product.Name = productDto.Name;
            product.Brand = productDto.Brand;
            product.Description = productDto.Description;
            product.Price = productDto.Price;
            product.Category = productDto.Category;
            product.ImageFileName = newFileName;

            context.SaveChanges();
            return RedirectToAction("Index","Product");
        }


        public IActionResult Delete(int id)
        {
            var product = context.Products.Find(id);
            if (product == null) { 
            return RedirectToAction("Index","Product");
            }

            string imageFullPath = environment.WebRootPath + "/Products/" + product.ImageFileName;
            System.IO.File.Delete(imageFullPath) ;

            context.Products.Remove(product);
            context.SaveChanges(true);

            return RedirectToAction("Index", "Product");
        }
    }
}
