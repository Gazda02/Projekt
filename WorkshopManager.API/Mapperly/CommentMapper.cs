using Riok.Mapperly.Abstractions;
using WorkshopManager.API.DTOs;
using WorkshopManager.API.Models;

namespace WorkshopManager.API.Mapperly;

[Mapper]
public partial class CommentMapper
{
    public partial Comment Map(CommentDto commentDto);
    public partial CommentDto Map(Comment comment);
}