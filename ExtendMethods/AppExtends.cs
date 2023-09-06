namespace App.ExtendMethods
{
    public static class AppExtends
    {
        public static void AddStatusCodePage(this IApplicationBuilder app)
        {
            app.UseStatusCodePages(appError => {
            appError.Run(async context => {
                var response = context.Response;
                var code = response.StatusCode;
                var content = @$"<html>
                <head>
                <meta charset='UTF-8' />
                <title>Lỗi {code}</title>
                </html>
                <body>
                   <p style='color:red; font-size:30px'>
                     Có lỗi xảy ra: {code} 
                </body>";
                await response.WriteAsync(content);
            });
        }); //Middleware tạo ra nội dung Response trả về client khi ứng dụng gặp lỗi
        }
    }
}
