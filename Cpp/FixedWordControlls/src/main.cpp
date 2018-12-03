#include <Arduino.h>
#include "words.h"

constexpr uint8_t txEnPin = 2;
void loop1();
void show(int choice);

void setup() {
  Serial.begin(9600);
  Serial.setTimeout(INT32_MAX);
  pinMode(txEnPin, OUTPUT);
  //delay(1);
  digitalWrite(txEnPin, LOW);
  Serial1.begin(19200);
  //Serial1.setTimeout(INT32_MAX);

/*
  // test if pin A0 grounded, if so give menu
  pinMode(A0, INPUT_PULLUP);
  if (digitalRead(A0)==HIGH) {
    while(true) {loop1();}
  }
*/
}
/*
void loop() 
{
  for (int i=2; i<wordNum; i++) {
    show(i);
    delay(120000UL);
  }
}
*/

int slideNum = 0;

void skipToNewLine()
{
  /*
  char c;
  do {
    Serial.readBytes(&c, 1);
  } while (c!='\n');
  */
}

void loop() {
  // put your main code here, to run repeatedly:
  show(slideNum);
  Serial.print(slideNum);
  Serial.print(": ");
  Serial.println(names[slideNum]);

  char buffer[80];
  char *curChar = buffer;

  //char command;
  int choice;
  size_t bytesRead = Serial.readBytesUntil('\n', (uint8_t *)buffer, sizeof(buffer)-1); //(&command, 1);
  buffer[bytesRead] = '\0';

  while(true) {
    switch(*curChar++) {
    case 'N':
    case 'n':
      slideNum++;
      if (slideNum >= wordNum) {
        Serial.println("No more slides.");
        slideNum = wordNum - 1;
      }
      skipToNewLine();
      return;

    case 'P':
    case 'p':
      slideNum--;
      if (slideNum < 0) {
        Serial.println("No more slides.");
        slideNum = 0;
      }
      skipToNewLine();
      return;

    case 'G':
    case 'g':
      while (*curChar == ' ' && *curChar != '\0') {
        curChar++;
      }
      if (isDigit(*curChar)) {
        choice = atoi(curChar);
        if (choice >=0 && choice < wordNum) {
          slideNum = choice;
        } else {
          Serial.println("No such slide.");
        }
        skipToNewLine();
        return;
      } else {
        Serial.println("Expected number of slide.");
      }

    case 'L':
    case 'l':
      for (int i=0; i<wordNum; i++) {
        Serial.print(i);
        Serial.print(": ");
        Serial.println(names[i]);
      }
      Serial.println();
      skipToNewLine();
      return;

    case '?':
      Serial.println("N - next slide");
      Serial.println("P - previous slide");
      Serial.println("L - list slides");
      Serial.println("G <num> goto slide number <num>");
      skipToNewLine();
      return;

    case '\r':
    case '\n':
      return;

    case ' ':
      break; // try next letter

    default:
      Serial.println("Unrecognized command, use ? for help");
      return;
    }
  }

}


void show(int choice)
{
  digitalWrite(txEnPin, HIGH);
  Serial1.println();
  for (int i=0; i<40; i++) {
    uint8_t c = '1' + bitmaps[choice][i];
    Serial1.write(&c,1);
    //Serial.write(&c,1);
  }
  //Serial.println();
  Serial1.println();
  Serial1.flush();
  digitalWrite(txEnPin, LOW);

}
