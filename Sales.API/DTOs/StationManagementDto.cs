using System.ComponentModel.DataAnnotations;

namespace Sales.API.DTOs;

public class StationTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<int> CategoryIds { get; set; } = new();
    public List<StationDto> Stations { get; set; } = new();
}

public class StationTypeCreateDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }
}

public class StationTypeUpdateDto
{
    [MaxLength(255)]
    public string? Name { get; set; }

    [MaxLength(255)]
    public string? Description { get; set; }
}

public class StationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TypeId { get; set; }
    public string? TypeName { get; set; }
}

public class StationCreateDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int TypeId { get; set; }
}

public class CoverageAssignDto
{
    public List<int> CategoryIds { get; set; } = new();
}
