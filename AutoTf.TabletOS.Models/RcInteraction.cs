using System.Device.Spi;
using AutoTf.TabletOS.Models.Interfaces;
using Iot.Device.Card.Mifare;
using Iot.Device.Mfrc522;
using Iot.Device.Rfid;

namespace AutoTf.TabletOS.Models;

public class RcInteraction : IRcInteractions
{
	private readonly MfRc522 _rfid;
	private CancellationTokenSource _cancellationTokenSource;
	private bool _isCardPresent;
	private string? _lastCardUid;
	
	public event EventHandler<string> CardInserted;
	public event EventHandler<string> CardRemoved;
	public event EventHandler<string> CardRead;
	
	public RcInteraction(int spiBusId, int chipSelectLine, int resetPin)
	{
		SpiConnectionSettings spiSettings = new SpiConnectionSettings(spiBusId, chipSelectLine)
		{
			ClockFrequency = 500_000,
			Mode = SpiMode.Mode0
		};
		
		SpiDevice spiDevice = SpiDevice.Create(spiSettings);
		_rfid = new MfRc522(spiDevice, pinReset: resetPin);
	}
	
	public void StartMonitoring()
	{
		_cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = _cancellationTokenSource.Token;

		Thread monitoringThread = new Thread(() =>
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					if (_rfid.ListenToCardIso14443TypeA(out Data106kbpsTypeA card, TimeSpan.FromMilliseconds(500)))
					{
						string currentUid = BitConverter.ToString(card.NfcId);

						if (!_isCardPresent || _lastCardUid != currentUid)
						{
							_isCardPresent = true;
							_lastCardUid = currentUid;
							CardInserted?.Invoke(this, currentUid);
						}
						
						CardRead?.Invoke(this, currentUid);
					}
					else if (_isCardPresent)
					{
						_isCardPresent = false;
						CardRemoved.Invoke(this, _lastCardUid!);
						_lastCardUid = null;
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error during RFID operation: {ex.Message}");
				}

				Thread.Sleep(500);
			}
		});

		monitoringThread.IsBackground = true;
		monitoringThread.Start();
	}
	
	public string ReadCardContent(byte[]? key = null)
	{
		key ??= new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }; 
		if (!_rfid.ListenToCardIso14443TypeA(out Data106kbpsTypeA card, TimeSpan.FromSeconds(1)))
		{
			return "No card detected.";
		}

		Console.WriteLine($"Card detected: UID={BitConverter.ToString(card.NfcId)}");

		byte[] buffer = new byte[16];
		var result = string.Empty;

		try
		{
			for (int block = 0; block < 64; block++) // Mifare Classic has 64 blocks
			{
				if (_rfid.MifareAuthenticate(key, MifareCardCommand.AuthenticationA, (byte)block, card.NfcId) != Status.Ok)
				{
					Console.WriteLine($"Authentication failed for block {block}");
					continue;
				}

				if (_rfid.SendAndReceiveData(MfrcCommand.Transceive, new byte[] { (byte)MifareCardCommand.Read16Bytes, (byte)block }, buffer) == Status.Ok)
				{
					result += $"Block {block}: {BitConverter.ToString(buffer)}\n";
				}
				else
				{
					Console.WriteLine($"Failed to read block {block}");
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error reading card: {ex.Message}");
		}

		return string.IsNullOrWhiteSpace(result) ? "No readable data on card." : result;
	}
	
	public void StopMonitoring()
	{
		_cancellationTokenSource?.Cancel();
		_rfid?.Dispose();
	}
}