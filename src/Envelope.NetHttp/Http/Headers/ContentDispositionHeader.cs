using System.Net.Http.Headers;

namespace Envelope.NetHttp.Http.Headers;

public class ContentDispositionHeader
{
	public DateTimeOffset? CreationDate { get; set; }
	public string? DispositionType { get; set; }
	public string? FileName { get; set; }
	public string? FileNameStar { get; set; }
	public DateTimeOffset? ModificationDate { get; set; }
	public string? Name { get; set; }
	public List<NameValueHeader>? Parameters { get; }
	public DateTimeOffset? ReadDate { get; set; }
	public long? Size { get; set; }

	public ContentDispositionHeaderValue ToContentDispositionHeaderValue()
	{
		if (string.IsNullOrWhiteSpace(DispositionType))
			throw new InvalidOperationException($"{nameof(DispositionType)} == null");

		var contentDispositionHeaderValue = new ContentDispositionHeaderValue(DispositionType);

		if (CreationDate.HasValue)
			contentDispositionHeaderValue.CreationDate = CreationDate;

		if (!string.IsNullOrWhiteSpace(FileName))
			contentDispositionHeaderValue.FileName = FileName;

		if (!string.IsNullOrWhiteSpace(FileNameStar))
			contentDispositionHeaderValue.FileNameStar = FileNameStar;

		if (ModificationDate.HasValue)
			contentDispositionHeaderValue.ModificationDate = ModificationDate;

		if (!string.IsNullOrWhiteSpace(Name))
			contentDispositionHeaderValue.Name = Name;

		if (Parameters != null && 0 < Parameters.Count)
			foreach (var parameter in Parameters)
				contentDispositionHeaderValue.Parameters.Add(parameter.ToNameValueHeaderValue());

		if (ReadDate.HasValue)
			contentDispositionHeaderValue.ReadDate = ReadDate;

		if (Size.HasValue)
			contentDispositionHeaderValue.Size = Size;

		return contentDispositionHeaderValue;
	}
}
