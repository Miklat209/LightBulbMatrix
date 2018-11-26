#include <Arduino.h>
#include "words.h"

constexpr uint8_t txEnPin = 2;
void loop1();
void show(int choice);

void setup() {
  Serial.begin(9600);
  Serial.setTimeout(INT32_MAX);
  pinMode(txEnPin, OUTPUT);
  delay(1);
  digitalWrite(txEnPin, LOW);
  Serial1.begin(19200);
  Serial1.setTimeout(INT32_MAX);

  // test if pin A0 grounded, if so give menu
  pinMode(A0, INPUT_PULLUP);
  if (digitalRead(A0)==HIGH) {
    while(true) {loop1();}
  }

}

void loop() 
{
  for (int i=2; i<wordNum; i++) {
    show(i);
    delay(120000UL);
  }
}

void loop1() {
  // put your main code here, to run repeatedly:
  Serial.println("Enter number:");

  long choice = Serial.parseInt();
  if (choice < 0 || choice >= wordNum) {
    Serial.print("Valid values: 0 - ");
    Serial.println(wordNum-1);
    return;
  }
  show((int)choice);
}


void show(int choice)
{
  digitalWrite(txEnPin, HIGH);
  Serial1.println();
  for (int i=0; i<30; i++) {
    uint8_t c = '1' + words[choice][i];
    Serial1.write(&c,1);
    //Serial.write(&c,1);
  }
  //Serial.println();
  Serial1.println();
  Serial1.flush();
  digitalWrite(txEnPin, LOW);

}
