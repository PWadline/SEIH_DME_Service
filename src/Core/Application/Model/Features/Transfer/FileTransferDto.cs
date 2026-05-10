using Microsoft.AspNetCore.Http;

namespace Core.Application.Model.Features;

public class FileTransferDto
{
    public string Label { get; set; } = ""; // Fichier_001
    public string Path { get; set; } = "";  // chemin original
}

