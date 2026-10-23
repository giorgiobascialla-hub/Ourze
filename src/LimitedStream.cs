using System;using System.IO;using System.IO.Compression;
namespace HdrPilot {
public sealed class LimitedStream : Stream {
 readonly Stream input;readonly long limit,expected;long read;
 public LimitedStream(Stream input,long limit,long expected=-1){if(input==null)throw new ArgumentNullException("input");if(limit<0||expected>limit)throw new IOException("Stream exceeds the allowed size.");this.input=input;this.limit=limit;this.expected=expected;}
 public static Stream Open(ZipArchiveEntry entry,long limit){if(entry.Length>limit)throw new IOException("Archive entry exceeds the allowed size.");return new LimitedStream(entry.Open(),limit,entry.Length);}
 public override int Read(byte[] buffer,int offset,int count){if(buffer==null)throw new ArgumentNullException("buffer");if(offset<0||count<0||offset>buffer.Length-count)throw new ArgumentOutOfRangeException();if(count==0)return 0;int n=input.Read(buffer,offset,(int)Math.Min(count,Math.Min(Int32.MaxValue,limit-read+1)));read+=n;if(read>limit)throw new IOException("Stream exceeds the allowed size.");if(n==0&&expected>=0&&read!=expected)throw new IOException("Stream size does not match the declared length.");return n;}
 public override bool CanRead{get{return input.CanRead;}}public override bool CanSeek{get{return false;}}public override bool CanWrite{get{return false;}}
 public override long Length{get{throw new NotSupportedException();}}public override long Position{get{return read;}set{throw new NotSupportedException();}}
 public override void Flush(){}public override long Seek(long offset,SeekOrigin origin){throw new NotSupportedException();}public override void SetLength(long value){throw new NotSupportedException();}public override void Write(byte[] buffer,int offset,int count){throw new NotSupportedException();}
 protected override void Dispose(bool disposing){if(disposing)input.Dispose();base.Dispose(disposing);}
}
public static class AppIdentity {public static string Version{get{return typeof(AppIdentity).Assembly.GetName().Version.ToString(3);}}public static string UserAgent{get{return "HDLSS5/"+Version;}}}
}
