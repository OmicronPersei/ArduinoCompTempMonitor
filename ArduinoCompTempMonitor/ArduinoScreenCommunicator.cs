using System;
using System.IO.Ports;
using System.Collections.Generic;

/*
 * 
 * This class will handle sending data over the serial USB port to the Arduino, of which will handle displaying to it's connected LCD screen.
 * The class constructor will specify which com port the arduino is located on, and the LCD screen dimension (number of characters per line).
 * 
 * */

public class ArduinoScreenCommunicator
{
    private SerialPort _serialPort;
    private string _endLineChar;
    private string _beginTransmissionChar;
    private string _endTransmissionChar;
    private int _maxLineLength;
    
    public ArduinoScreenCommunicator(string portName, int maxLineLength)
	{
        _maxLineLength = maxLineLength;
        
        _serialPort = new SerialPort(portName, 9600);

        _endLineChar = "\n";
        _beginTransmissionChar = ((char)1).ToString();
        _endTransmissionChar = ((char)2).ToString();
	}

    private bool AttemptToOpen()
    {
        //Attempt to open the serial port.
        if (!_serialPort.IsOpen)
        {
            try
            {
                _serialPort.Open();

                return _serialPort.IsOpen;
            }
            catch
            {
                return false;
            }
        }
        else
        {
            return true;
        }
    }

    //public bool IsOpened()
    //{
    //    return _serialPort.IsOpen;
    //}

    public void SendLines(List<string> theLines)
    {
        if (AttemptToOpen())
        {
            _serialPort.Write(_beginTransmissionChar.ToCharArray(), 0, 1);

            foreach (string aLine in theLines)
            {
                //First we need to check that each line is  <= the specified maxLineLength.  If over, truncate.
                string buff = aLine;
                if (buff.Length > _maxLineLength)
                    buff = buff.Substring(0, _maxLineLength);
                
                //Now append the end line char.  This is important so the Arduino knows where the end of the line is.
                buff = buff + _endLineChar;

                //We stored the line as a string so we can convert it to a char[], meaning the null terminating character of variable type string isn't present.
                _serialPort.Write(buff.ToCharArray(), 0, buff.Length);
            }

            //Finished writing all the lines, now write the end transmission character so the Arduino knows it's finished receiving lines.
            _serialPort.Write(_endTransmissionChar.ToCharArray(), 0, 1);
        }

        //Close();
    }

    public void Close()
    {
        if (_serialPort.IsOpen)
            _serialPort.Close();
    }
}
