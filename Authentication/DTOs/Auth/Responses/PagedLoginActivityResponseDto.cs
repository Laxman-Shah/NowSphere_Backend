namespace smartApi.Authentication.DTOs.Auth.Responses;

public sealed class PagedLoginActivityResponseDto
{
    public IReadOnlyCollection<LoginActivityResponseDto> Items { get; set; }
        = Array.Empty<LoginActivityResponseDto>();

    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}