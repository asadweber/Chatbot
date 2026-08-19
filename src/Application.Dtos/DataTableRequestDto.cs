namespace Application.Dtos;

public class DataTableRequestDto
{
    public int Draw { get; set; }
    public int Start { get; set; }
    public int Length { get; set; }
    public string? SearchValue { get; set; }
    public string? SortColumn { get; set; }
    public string? SortDirection { get; set; }
}
