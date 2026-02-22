using Application.Extensions;
using FluentValidation;

namespace Application.Shared
{
    public class SharedFileResponse
    {
        public byte[] File { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
    }

    public class FileRequestDto
    {
        public string FileBase64 { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
    }

    public class FileRequestValidator : AbstractValidator<FileRequestDto>
    {
        public FileRequestValidator()
        {
            RuleFor(p => p.FileBase64).Required();
            RuleFor(p => p.FileName).Required().Max(50);
            RuleFor(p => p.FileType).Required().Max(5);
        }
    }
}
