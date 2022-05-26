using Envelope.Web.Logging;

namespace Envelope.NetHttp;

public interface IRequestResponseLogger
{
	Func<RequestDto, string?, Guid, CancellationToken, Task>? OnBeforeRequestSendAsStringAsync { get; }
	Func<RequestDto, byte[]?, Guid, CancellationToken, Task>? OnBeforeRequestSendAsByteArrayAsync { get; }
	Func<RequestDto, Stream?, Guid, CancellationToken, Task>? OnBeforeRequestSendAsStreamAsync { get; }
	Func<ResponseDto, string?, Guid, CancellationToken, Task>? OnAfterResponseReceivedAsStringAsync { get; }
	Func<ResponseDto, byte[]?, Guid, CancellationToken, Task>? OnAfterResponseReceivedAsByteArrayAsync { get; }
	Func<ResponseDto, Stream?, Guid, CancellationToken, Task>? OnAfterResponseReceivedAsStreamAsync { get; }
}
