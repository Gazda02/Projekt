using Riok.Mapperly.Abstractions;
using WorkshopManager.API.DTOs;
using WorkshopManager.API.Models;

namespace WorkshopManager.API.Mapperly;

[Mapper]
public partial class TaskMapper
{
        public partial ServiceTask Map(TaskDto taskDto);
        public partial TaskDto Map(ServiceTask task);
}