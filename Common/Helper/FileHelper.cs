using Common.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helper
{
    public class FileHelper
    {
        public static ResponseModel UploadFile(IFormFile file, string folderUpload, string uploadPath)
        {
            var response = new ResponseModel() { IsSuccessful = true };
            try
            {
                string fileName = Guid.NewGuid().ToString() + file.FileName;
                string fullPath = Path.Combine(folderUpload, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                response.Data = new
                {
                    Path = uploadPath + fileName,
                    Name = fileName
                };
            }
            catch(Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }
        public static ResponseModel DeleteFile(string fullPath)
        {
            var response = new ResponseModel() { IsSuccessful = true };
            if (System.IO.File.Exists(fullPath))
            {
                try
                {
                    System.IO.File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    response.IsSuccessful = false;
                    response.Message = ex.Message;
                }
            }
            return response;
        }
    }
}
