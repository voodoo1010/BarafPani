using System;

namespace Unity.Services.Vivox
{
    internal class VivoxApiException : Exception
    {
        public int StatusCode { get; private set; }
        public string RequestId { get; private set; }

        public VivoxApiException(int statusCode)
            : base($"{GetErrorString(statusCode)} ({statusCode})")
        {
            StatusCode = statusCode;
        }

        public VivoxApiException(int statusCode, string requestId)
            : base($"{GetErrorString(statusCode)} ({statusCode})")
        {
            StatusCode = statusCode;
            RequestId = requestId;
        }

        public VivoxApiException(int statusCode, Exception inner)
            : base($"{GetErrorString(statusCode)} ({statusCode})", inner)
        {
        }

        public static string GetErrorString(int statusCode)
        {
            if (statusCode <= (int)vx_tts_status.tts_error_invalid_engine_type)
                return VivoxCoreInstance.vx_get_tts_status_string((vx_tts_status)statusCode);
            else
                return VivoxCoreInstance.vx_get_error_string(statusCode);
        }
    }
}
