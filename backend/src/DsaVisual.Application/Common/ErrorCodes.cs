namespace DsaVisual.Application.Common;

/// <summary>
/// Danh mục Error Code tập trung — khớp 100% API_REFERENCE.md §2.2 (Error Code Catalog).
/// CẤM phát minh mã mới ngoài danh sách; ngoại lệ phải bổ sung vào bảng kèm phiên bản (SDD §5.3.3).
/// </summary>
public static class ErrorCodes
{
    // 400
    public const string VALIDATION_FAILED = "VALIDATION_FAILED";
    public const string WEAK_PASSWORD = "WEAK_PASSWORD";
    public const string DOMAIN_NOT_ALLOWED = "DOMAIN_NOT_ALLOWED";
    public const string INVALID_EMAIL = "INVALID_EMAIL";
    public const string OLD_PASSWORD_WRONG = "OLD_PASSWORD_WRONG";
    public const string PASSWORD_SAME = "PASSWORD_SAME";
    public const string RESET_TOKEN_INVALID = "RESET_TOKEN_INVALID";
    public const string SIMULATION_KEY_INVALID = "SIMULATION_KEY_INVALID";
    public const string INPUT_INVALID = "INPUT_INVALID";
    public const string QUESTION_ANSWER_MISMATCH = "QUESTION_ANSWER_MISMATCH";
    public const string UPLOAD_INVALID_TYPE = "UPLOAD_INVALID_TYPE";
    public const string UPLOAD_TOO_LARGE = "UPLOAD_TOO_LARGE";

    // 401
    public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
    public const string UNAUTHORIZED = "UNAUTHORIZED";
    public const string TOKEN_EXPIRED = "TOKEN_EXPIRED";
    public const string REFRESH_INVALID = "REFRESH_INVALID";

    // 403
    public const string ACCOUNT_LOCKED = "ACCOUNT_LOCKED";
    public const string ACCOUNT_DISABLED = "ACCOUNT_DISABLED";
    public const string HEARTS_EMPTY = "HEARTS_EMPTY";
    public const string FORBIDDEN = "FORBIDDEN";

    // 404
    public const string NOT_FOUND = "NOT_FOUND";

    // 409
    public const string EMAIL_EXISTS = "EMAIL_EXISTS";
    public const string TOPIC_HAS_LESSONS = "TOPIC_HAS_LESSONS";
    public const string LESSON_HAS_EXERCISES = "LESSON_HAS_EXERCISES";
    public const string DUPLICATE_SIMULATION = "DUPLICATE_SIMULATION";

    // 422
    public const string INPUT_TOO_LARGE = "INPUT_TOO_LARGE";
    public const string SUBMISSION_IN_PROGRESS = "SUBMISSION_IN_PROGRESS";
    public const string EXERCISE_CLOSED = "EXERCISE_CLOSED";
    public const string LADDER_LOCKED = "LADDER_LOCKED";
    public const string INSUFFICIENT_GEMS = "INSUFFICIENT_GEMS";
    public const string QUEST_ALREADY_CLAIMED = "QUEST_ALREADY_CLAIMED";

    // 429
    public const string RATE_LIMITED = "RATE_LIMITED";

    // 500 / 503
    public const string INTERNAL_ERROR = "INTERNAL_ERROR";
    public const string SERVICE_UNAVAILABLE = "SERVICE_UNAVAILABLE";

    /// <summary>
    /// Ánh xạ ErrorCode → HTTP status theo catalog (API_REFERENCE.md §2.2).
    /// </summary>
    public static int GetHttpStatus(string code) => code switch
    {
        VALIDATION_FAILED or WEAK_PASSWORD or DOMAIN_NOT_ALLOWED or INVALID_EMAIL
            or OLD_PASSWORD_WRONG or PASSWORD_SAME or RESET_TOKEN_INVALID
            or SIMULATION_KEY_INVALID or INPUT_INVALID or QUESTION_ANSWER_MISMATCH
            or UPLOAD_INVALID_TYPE or UPLOAD_TOO_LARGE => 400,
        INVALID_CREDENTIALS or UNAUTHORIZED or TOKEN_EXPIRED or REFRESH_INVALID => 401,
        ACCOUNT_LOCKED or ACCOUNT_DISABLED or HEARTS_EMPTY or FORBIDDEN => 403,
        NOT_FOUND => 404,
        EMAIL_EXISTS or TOPIC_HAS_LESSONS or LESSON_HAS_EXERCISES or DUPLICATE_SIMULATION => 409,
        INPUT_TOO_LARGE or SUBMISSION_IN_PROGRESS or EXERCISE_CLOSED or LADDER_LOCKED
            or INSUFFICIENT_GEMS or QUEST_ALREADY_CLAIMED => 422,
        RATE_LIMITED => 429,
        SERVICE_UNAVAILABLE => 503,
        _ => 500
    };
}
