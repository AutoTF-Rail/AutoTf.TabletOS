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
			ClockFrequency = 5_000_000
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
		bool res;
        Data106kbpsTypeA card;

        // Wait for a card to be detected
        do
        {
            res = _rfid.ListenToCardIso14443TypeA(out card, TimeSpan.FromSeconds(2));
            Thread.Sleep(res ? 0 : 200);
        }
        while (!res);

        Console.WriteLine($"Card detected: UID={BitConverter.ToString(card.NfcId)}");

        // Initialize the MifareCard
        MifareCard mifare = new MifareCard(_rfid, 0)
        {
            SerialNumber = card.NfcId,
            Capacity = MifareCardCapacity.Mifare1K,
            KeyA = MifareCard.DefaultKeyA.ToArray(),
            KeyB = MifareCard.DefaultKeyB.ToArray()
        };

        string result = string.Empty;

        // Loop through blocks
        for (int block = 0; block < 64; block++)
        {
            mifare.BlockNumber = (byte)block;

            // Authenticate the block
            mifare.Command = MifareCardCommand.AuthenticationA; // Try Key A
            int ret = mifare.RunMifareCardCommand();

            if (ret < 0)
            {
                mifare.ReselectCard();
                Console.WriteLine($"Authentication failed for block {block}. Retrying with Key B.");

                // Try Key B if Key A fails
                mifare.Command = MifareCardCommand.AuthenticationB;
                ret = mifare.RunMifareCardCommand();

                if (ret < 0)
                {
                    mifare.ReselectCard();
                    Console.WriteLine($"Error authenticating block {block}");
                    continue;
                }
            }

            // Read the block
            mifare.Command = MifareCardCommand.Read16Bytes;
            ret = mifare.RunMifareCardCommand();

            if (ret >= 0 && mifare.Data is object)
            {
                result += $"Block {block}: {BitConverter.ToString(mifare.Data)}\n";
            }
            else
            {
                mifare.ReselectCard();
                Console.WriteLine($"Error reading block {block}");
            }
        }

        return string.IsNullOrWhiteSpace(result) ? "No readable data on card." : result;
	}
	
	public void StopMonitoring()
	{
		_cancellationTokenSource?.Cancel();
		_rfid?.Dispose();
	}
}