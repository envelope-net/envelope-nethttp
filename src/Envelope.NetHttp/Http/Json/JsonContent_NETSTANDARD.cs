#if NETSTANDARD2_0 || NETSTANDARD2_1
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace System.Net.Http.Json;

public sealed partial class JsonContent : HttpContent
{
	internal const string JsonMediaType = "application/json";
	internal const string JsonType = "application";
	internal const string JsonSubtype = "json";
	private static MediaTypeHeaderValue DefaultMediaType
		=> new(JsonMediaType) { CharSet = "utf-8" };

	internal static readonly JsonSerializerSettings _defaultSerializerSettings = new();

	private readonly JsonSerializerSettings? _jsonSerializerSettings;
	public Type ObjectType { get; }
	public object? Value { get; }

	private JsonContent(object? inputValue, Type inputType, MediaTypeHeaderValue? mediaType, JsonSerializerSettings? options)
	{
		if (inputType == null)
			throw new ArgumentNullException(nameof(inputType));

		if (inputValue != null && !inputType.IsAssignableFrom(inputValue.GetType()))
			throw new ArgumentException(string.Format("The specified type {0} must derive from the specific value's type {1}.", inputType, inputValue.GetType()));

		Value = inputValue;
		ObjectType = inputType;
		Headers.ContentType = mediaType ?? DefaultMediaType;
		_jsonSerializerSettings = options ?? _defaultSerializerSettings;
	}

	public static JsonContent Create<T>(T inputValue, MediaTypeHeaderValue? mediaType = null, JsonSerializerSettings? options = null)
		=> Create(inputValue, typeof(T), mediaType, options);

	public static JsonContent Create(object? inputValue, Type inputType, MediaTypeHeaderValue? mediaType = null, JsonSerializerSettings? options = null)
		=> new(inputValue, inputType, mediaType, options);

	protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
		=> SerializeToStreamAsyncCoreAsync(stream, CancellationToken.None);

	protected override bool TryComputeLength(out long length)
	{
		length = 0;
		return false;
	}

	private async Task SerializeToStreamAsyncCoreAsync(Stream targetStream, CancellationToken cancellationToken)
	{
		Encoding? targetEncoding = GetEncoding(Headers.ContentType?.CharSet);

		// Wrap provided stream into a transcoding stream that buffers the data transcoded from utf-8 to the targetEncoding.
		if (targetEncoding != null && targetEncoding != Encoding.UTF8)
		{
			using var transcodingStream = new TranscodingWriteStream(targetStream, targetEncoding);
			using var writer = new StreamWriter(transcodingStream);
			using var jsonWriter = new JsonTextWriter(writer);
			var jsonSerializer = JsonSerializer.Create(_jsonSerializerSettings);
			jsonSerializer.Serialize(jsonWriter, Value, ObjectType);
			jsonWriter.Flush();

			// The transcoding streams use Encoders and Decoders that have internal buffers. We need to flush these
			// when there is no more data to be written. Stream.FlushAsync isn't suitable since it's
			// acceptable to Flush a Stream (multiple times) prior to completion.
			await transcodingStream.FinalWriteAsync(cancellationToken).ConfigureAwait(false);
		}
		else
		{
			using var writer = new StreamWriter(targetStream, targetEncoding, 1024, true);
			using var jsonWriter = new JsonTextWriter(writer);
			var jsonSerializer = JsonSerializer.Create(_jsonSerializerSettings);
			jsonSerializer.Serialize(jsonWriter, Value, ObjectType);
			jsonWriter.Flush();
		}
	}

	internal static Encoding? GetEncoding(string? charset)
	{
		Encoding? encoding = null;

		if (charset != null)
		{
			try
			{
				// Remove at most a single set of quotes.
				if (charset.Length > 2 && charset[0] == '\"' && charset[charset.Length - 1] == '\"')
				{
					encoding = Encoding.GetEncoding(charset.Substring(1, charset.Length - 2));
				}
				else
				{
					encoding = Encoding.GetEncoding(charset);
				}
			}
			catch (ArgumentException e)
			{
				throw new InvalidOperationException("The character set provided in ContentType is invalid.", e);
			}
		}

		return encoding;
	}
}
#endif
