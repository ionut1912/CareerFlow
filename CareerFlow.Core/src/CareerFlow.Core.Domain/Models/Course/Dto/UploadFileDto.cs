namespace CareerFlow.Core.Domain.Models.Course.Dto;

public sealed record UploadFileDto(
    string FileName,
    string ContentType,
    Stream Content);