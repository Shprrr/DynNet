using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace DynServer
{
	// Original source: http://blog.stephencleary.com/2009/04/sample-code-length-prefix-message.html
	/// <summary>
	/// Maintains the necessary buffers for applying a length-prefix message framing protocol over a stream.
	/// </summary>
	/// <remarks>
	/// <para>Create one instance of this class for each incoming stream, and assign a handler to <see cref="MessageArrived"/>.
	/// As bytes arrive at the stream, pass them to <see cref="DataReceived"/>, which will invoke <see cref="MessageArrived"/> as necessary.</para>
	/// <para>If <see cref="DataReceived"/> raises <see cref="ProtocolViolationException"/>, then the stream data should be considered invalid.
	/// After that point, no methods should be called on that <see cref="PacketProtocol"/> instance.</para>
	/// <para>This class uses a 4-byte signed integer length prefix, which allows for message sizes up to 2 GB.
	/// Keepalive messages are supported as messages with a length prefix of 0 and no message data.</para>
	/// <para>This is EXAMPLE CODE! It is not particularly efficient; in particular,
	/// if this class is rewritten so that a particular interface is used (e.g., Socket's IAsyncResult methods), some buffer copies become unnecessary and may be removed.</para>
	/// </remarks>
	public class PacketProtocol
	{
		/// <summary>
		/// Wraps a message. The wrapped message is ready to send to a stream.
		/// </summary>
		/// <remarks>
		/// <para>Generates a length prefix for the message and returns the combined length prefix and message.</para>
		/// </remarks>
		/// <param name="message">The message to send.</param>
		/// <returns>Wrapped message.</returns>
		public static byte[] WrapMessage(byte[] message)
		{
			// Get the length prefix for the message
			byte[] lengthPrefix = BitConverter.GetBytes(message.Length);

			// Concatenate the length prefix and the message
			byte[] ret = new byte[lengthPrefix.Length + message.Length];
			lengthPrefix.CopyTo(ret, 0);
			message.CopyTo(ret, lengthPrefix.Length);

			return ret;
		}

		/// <summary>
		/// Wraps a message. The wrapped message is ready to send to a stream.
		/// </summary>
		/// <remarks>
		/// <para>Generates a length prefix for the message and returns the combined length prefix and message.</para>
		/// </remarks>
		/// <param name="message">The message to send.</param>
		/// <returns>Wrapped message.</returns>
		public static byte[] WrapMessage(string message)
		{
			return WrapMessage(System.Text.Encoding.UTF8.GetBytes(message));
		}

		/// <summary>
		/// Wraps a keepalive (0-length) message. The wrapped message is ready to send to a stream.
		/// </summary>
		public static byte[] WrapKeepaliveMessage()
		{
			return BitConverter.GetBytes(0);
		}

		/// <summary>
		/// Initializes a new <see cref="PacketProtocol"/>, limiting message sizes to the given maximum size.
		/// </summary>
		/// <param name="maxMessageSize">The maximum message size supported by this protocol. This may be less than or equal to zero to indicate no maximum message size.</param>
		public PacketProtocol(int maxMessageSize)
		{
			// We allocate the buffer for receiving message lengths immediately
			lengthBuffer = new byte[sizeof(int)];
			this.maxMessageSize = maxMessageSize;
		}

		/// <summary>
		/// The buffer for the length prefix; this is always 4 bytes long.
		/// </summary>
		private byte[] lengthBuffer;

		/// <summary>
		/// The buffer for the data; this is null if we are receiving the length prefix buffer.
		/// </summary>
		private byte[] dataBuffer;

		/// <summary>
		/// The number of bytes already read into the buffer (the length buffer if <see cref="dataBuffer"/> is null, otherwise the data buffer).
		/// </summary>
		private int bytesReceived;

		/// <summary>
		/// The maximum size of messages allowed.
		/// </summary>
		private int maxMessageSize;

		/// <summary>
		/// Indicates the completion of a message read from the stream.
		/// </summary>
		/// <remarks>
		/// <para>This may be called with an empty message, indicating that the other end had sent a keepalive message. This will never be called with a null message.</para>
		/// <para>This event is invoked from within a call to <see cref="DataReceived"/>. Handlers for this event should not call <see cref="DataReceived"/>.</para>
		/// </remarks>
		public Action<byte[], object> MessageArrived { get; set; }

		/// <summary>
		/// Parameter to pass to the Action when the message will arrive completely.
		/// </summary>
		public object Parameter { get; set; }

		/// <summary>
		/// Notifies the <see cref="PacketProtocol"/> instance that incoming data has been received from the stream. This method will invoke <see cref="MessageArrived"/> as necessary.
		/// </summary>
		/// <remarks>
		/// <para>This method may invoke <see cref="MessageArrived"/> zero or more times.</para>
		/// <para>Zero-length receives are ignored. Many streams use a 0-length read to indicate the end of a stream, but <see cref="PacketProtocol"/> takes no action in this case.</para>
		/// </remarks>
		/// <param name="data">The data received from the stream. Cannot be null.</param>
		/// <returns>If the data received was complete.</returns>
		/// <exception cref="ProtocolViolationException">If the data received is not a properly-formed message.</exception>
		public bool DataReceived(byte[] data)
		{
			// Process the incoming data in chunks, as the ReadCompleted requests it

			// Logically, we are satisfying read requests with the received data, instead of processing the
			//  incoming buffer looking for messages.

			int i = 0;
			while (i != data.Length)
			{
				// Determine how many bytes we want to transfer to the buffer and transfer them
				int bytesAvailable = data.Length - i;
				if (dataBuffer != null)
				{
					// We're reading into the data buffer
					int bytesRequested = dataBuffer.Length - bytesReceived;

					// Copy the incoming bytes into the buffer
					int bytesTransferred = Math.Min(bytesRequested, bytesAvailable);
					Array.Copy(data, i, dataBuffer, bytesReceived, bytesTransferred);
					i += bytesTransferred;

					// Notify "read completion"
					if (ReadCompleted(bytesTransferred)) return true;
				}
				else
				{
					// We're reading into the length prefix buffer
					int bytesRequested = lengthBuffer.Length - bytesReceived;

					// Copy the incoming bytes into the buffer
					int bytesTransferred = Math.Min(bytesRequested, bytesAvailable);
					Array.Copy(data, i, lengthBuffer, bytesReceived, bytesTransferred);
					i += bytesTransferred;

					// Notify "read completion"
					if (ReadCompleted(bytesTransferred)) return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Encapsulate a read from a Stream to give to DataReceived.
		/// </summary>
		/// <param name="stream">Stream to read from.</param>
		public void DataReceived(NetworkStream stream)
		{
			bool received = false;
			while (!received)
			{
				byte[] data = new byte[maxMessageSize];
				try
				{
					stream.Read(data, 0, data.Length);
				}
				catch (ObjectDisposedException)
				{
					// Object already gone.
					return;
				}
				catch (Exception ex)
				{
					var ioEx = ex as System.IO.IOException;
					var socketEx = ex as SocketException;
					if (ioEx != null && ioEx.InnerException is SocketException)
						socketEx = (SocketException)ioEx.InnerException;

					if (socketEx != null)
						switch (socketEx.SocketErrorCode)
						{
							case SocketError.ConnectionAborted:
							case SocketError.ConnectionReset:
							case SocketError.Interrupted:
								return;
						}

					throw ex;
				}
				received = DataReceived(data);
			}
		}

		/// <summary>
		/// Called when a read completes. Parses the received data and calls <see cref="MessageArrived"/> if necessary.
		/// </summary>
		/// <param name="count">The number of bytes read.</param>
		/// <returns>If reading is completed.</returns>
		/// <exception cref="ProtocolViolationException">If the data received is not a properly-formed message.</exception>
		private bool ReadCompleted(int count)
		{
			// Get the number of bytes read into the buffer
			bytesReceived += count;

			if (dataBuffer == null)
			{
				// We're currently receiving the length buffer

				if (bytesReceived != sizeof(int))
				{
					// We haven't gotten all the length buffer yet: just wait for more data to arrive
				}
				else
				{
					// We've gotten the length buffer
					int length = BitConverter.ToInt32(lengthBuffer, 0);

					// Sanity check for length < 0
					if (length < 0)
						throw new ProtocolViolationException("Message length is less than zero");

					// Another sanity check is needed here for very large packets, to prevent denial-of-service attacks
					if (maxMessageSize > 0 && length > maxMessageSize)
						throw new ProtocolViolationException("Message length " + length.ToString(CultureInfo.InvariantCulture) + " is larger than maximum message size " + maxMessageSize.ToString(CultureInfo.InvariantCulture));

					// Zero-length packets are allowed as keepalives
					if (length == 0)
					{
						bytesReceived = 0;
						MessageArrived?.Invoke(new byte[0], Parameter);
						return true;
					}
					else
					{
						// Create the data buffer and start reading into it
						dataBuffer = new byte[length];
						bytesReceived = 0;
					}
				}
			}
			else
			{
				if (bytesReceived != dataBuffer.Length)
				{
					// We haven't gotten all the data buffer yet: just wait for more data to arrive
				}
				else
				{
					// We've gotten an entire packet
					MessageArrived?.Invoke(dataBuffer, Parameter);

					// Start reading the length buffer again
					dataBuffer = null;
					bytesReceived = 0;
					return true;
				}
			}

			return false;
		}
	}
}
