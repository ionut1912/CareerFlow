using CareerFlow.Core.Domain.Exceptions;
using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.Entities;

public class SystemDocument:Entity
{
    public string DocumentType{get; private set; }
    public string CurrentETag{get;private set;}

    private SystemDocument()
    {
        
    }
    
    public SystemDocument(string documentType,string currentETag)
    {
        if (string.IsNullOrWhiteSpace(documentType))
            throw new InvalidFieldException("DocumentType can not be empty");
        if(string.IsNullOrWhiteSpace(currentETag))
            throw new InvalidFieldException("CurrentETag can not be empty");
        DocumentType = documentType;
        CurrentETag = currentETag;
        CreatedAt = DateTime.UtcNow;
    }

    public static SystemDocument Create(string documentType, string currentETag)
    {
        return new SystemDocument(documentType, currentETag);
    }

    public void Update(string currentEtag)
    {
        if (currentEtag == CurrentETag)
        {
            throw new  DocumentEtagExistsException("CurrentETag can not be the same");
        }
        
        CurrentETag = currentEtag;
    }
    
}