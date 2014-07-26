#include <math.h>
#include <LiquidCrystal.h>

#define maxLines 20
#define lcdWidth 20
#define lcdHeight 4
#define scrollPin 5
#define ledPin 13

#define newlinechar '\n'
#define beginxmitchar 1
#define endxmitchar 2

int lastScrollVal = 0;
int analogVal = 0;
int calcScrollVal = 0;
//Pins 2, 3, 4, 5, 11, 12 reserved for LCD
//---PIN config---
//RS -> digital 12
//RW -> ground
//E  -> digital 11
//D4 -> digital 5
//D5 -> digital 4
//D6 -> digital 3
//D7 -> digital 2

int val; // Value read from the serial port
String values[maxLines];
String buffer;
int lineCount = 0;
//Initialize this variable to true such that, upon the first transmission, it knows to treat '\0'
//as the beginning of a transmisison, not the end.
boolean finishedReceiving = true;

int curLocation = 0;

LiquidCrystal lcd(12, 11, 5, 4, 3, 2);

void setup()
{
  //Setup the LED for the user to be notified of serial IO.
  pinMode(ledPin, OUTPUT);
  digitalWrite(ledPin, LOW);
  
  //Naturally, begin the serial IO.
  Serial.begin(9600);
  Serial.flush();
  
  //Initialize some variables.
  buffer = "";
  
  //Start the communcation with the LCD screen, specifying the screen dimensions here.
  lcd.begin(lcdWidth, lcdHeight);
  
}

void loop()
{
    //First, calculate the current scroll value
    calculateScrollValue();
    
    //Now determine if we need to update the display (user scrolled to a different part of the lines).
    handleScroll();
    
    if (Serial.available())
    {
      val = Serial.read();
      handleStringBuffer(val);
    }
}

//This function takes the analog value of the scroll pin and calculates a scroll value for the screen.
//It simply calls the calculation function, separated in the event of the need of change.
void calculateScrollValue()
{
    //Get the value from the analog input, then
    //scale the analog value and get a line location.
    calcScrollVal = calculateLocation(analogRead(scrollPin), 1024, lineCount);
}

//Used to calculate the line location from the position of the input voltage from the potentiometer.
//The potentiometer acts as a rough scroll wheel.
int calculateLocation(float analogval, float range, float lines) {
  
  float analogIndex;
  
  if (lines > lcdHeight)
  {
    analogIndex = (analogval * ((lines - lcdHeight + 1) / range));
  } else {
    analogIndex = 0;
  }
  return floor(analogIndex);
}

//This function calculates the line at which the LCD should display from the input potentiometer.
void handleScroll() {

    //Determine if the previous value differs from the currently calculated value.  If it does,
    //the user scrolled, and we want to update the display.
    if (calcScrollVal != lastScrollVal) {
      lastScrollVal = calcScrollVal;
      
      //We do not want to write lines UNTIL receiving a message because
      //the value for lineCount could be incorrect since we are still receiving the message.
      if (finishedReceiving) {
        printLines(calcScrollVal);
      }
    }
}

//This function handles reading the values from the serial IO.
void handleStringBuffer(int val) {
  if (val == newlinechar) {
    //Character indicates a new line, check if the buffer has data, if it does, store a line.
    
    if (buffer != "") {
      storeIntoLine();
    }
    
  } else if (val == beginxmitchar) {  
    //Computer host has indicated a new tramission, prepare to receive!!!
    //Clear all the lines and prepare for new lines.
    for (int i = 0; i < maxLines; ++i) {
		values[i] = "";
    }
    
    lineCount = 0;
    
    finishedReceiving = false;
      
    //write the LED to high, to notify the user that new data is being received.
    //digitalWrite(led, HIGH);
      
  } else if (val == endxmitchar) {
	  //Computer host has indicated the end of a transmission, do appropriate stuff.
      printLines(calcScrollVal);

	  finishedReceiving = true;
      
      //write the LED to low, notifying the user that new data is finished being received.
      //digitalWrite(led, LOW);
    
  } else {
    //Character as a part of a line.  If the buffer has NOT already reached it's maximum,
    //add it to the buffer.
    
    if (buffer.length() >= lcdWidth)
      storeIntoLine();
    
    buffer = buffer + (char) val;
  }
}

void storeIntoLine() {
  if (lineCount < maxLines) {
    //We need to pad the buffer with spaces so as to make the string 20 characters long.
    //This is to update the display without calling lcd.clear(), making the screen not flash as much.
    while (buffer.length() < lcdWidth) {
      buffer = buffer + " ";
    }
    
    values[lineCount] = buffer;
    ++lineCount;
    buffer = "";
  } else {
    //Serial.println("ERROR: MAX LINES ALREADY REACHED");
  }
}

void printLines(int startAt) {
  int linesOnLCD = 4;
  int curLineNum;
  
  //No longer calling this, now handled in a better way.
  //lcd.clear();
  
  for (int i = startAt; i < (startAt+linesOnLCD); ++i) {
    lcd.setCursor(0, (i - startAt));
    
    if (i < lineCount) {
      //Serial.println(values[i]);
      //Serial.println(valuesIndex);
      lcd.print(values[i]);
    } else {  
      //Index is outside of bounds, print blank line
      //Scroll bound calculation has been fixed such that this should not need to print any blank lines
      //EXCEPT when the total amount of supplied lines in the data payload is less than lines of the screen.
      //Serial.println("-");
      //(just print a blank line)
      lcd.print("                    ");
    }
  }
}
