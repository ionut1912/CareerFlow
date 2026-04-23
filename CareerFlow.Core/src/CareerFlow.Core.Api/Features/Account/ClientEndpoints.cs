using System.Text;

namespace CareerFlow.Core.Api.Features.Account;

public static class ClientEndpoints
{
    private static readonly string[] _assetLinkRelation = ["delegate_permission/common.handle_all_urls"];
    private static readonly string[] _certFingerprints = ["AMPRENTA_TA_SHA256_AICI"];

    public static void MapClientEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/.well-known/assetlinks.json", () =>
        {
            var assetLinks = new[]
            {
                new
                {
                    relation = _assetLinkRelation,
                    target = new
                    {
                        @namespace = "android_app",
                        package_name = "com.compania.careerflow",
                        sha256_cert_fingerprints = _certFingerprints
                    }
                }
            };
            return Results.Json(assetLinks);
        });

        endpoints.MapGet("/reset-password", () =>
        {
            const string html = """
                                <!DOCTYPE html>
                                <html lang='ro'>
                                  <head>
                                    <meta charset='UTF-8'>
                                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                                    <title>Career Flow</title>
                                    <style>
                                      body { font-family: sans-serif; text-align: center; padding: 40px 20px; background: #0f172a; color: white; }
                                      .btn { display: inline-block; background: #3b82f6; color: white; padding: 12px 24px; text-decoration: none; border-radius: 8px; margin: 10px; font-weight: bold; }
                                      p { color: #94a3b8; }
                                    </style>
                                  </head>
                                  <body>
                                    <h2>Aplicația nu a fost găsită</h2>
                                    <p>Pentru a-ți reseta parola, te rugăm să instalezi aplicația Career Flow.</p>
                                    <div style='margin-top: 30px;'>
                                      <a href='https://play.google.com/store/apps/details?id=com.compania.careerflow' class='btn'>Descarcă Android</a>
                                      <a href='https://apps.apple.com/app/idID_UL_AICI' class='btn'>Descarcă iOS</a>
                                    </div>
                                  </body>
                                </html>
                                """;

            return Results.Content(html, "text/html", Encoding.UTF8);
        });
    }
}
