namespace armada_test.Dto;

public record ApiResponse<T>(
    bool Success,
    string Message,
    T? Data = default
);