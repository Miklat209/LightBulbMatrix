#include <Arduino.h>
#include "words.h"

constexpr uint8_t txEnPin = 2;

void setup() {
  Serial.begin(9600);
  pinMode(txEnPin, OUTPUT);
  digitalWrite(txEnPin, LOW);
  Serial1.begin(19200);
  Serial1.setTimeout(INT32_MAX);
}

void loop() {
  // put your main code here, to run repeatedly:
  Serial.println("Enter number:");
  long choice = Serial.parseInt();
  if (choice < 0 || choice >= wordNum) {
    Serial.print("Valid values: 0 - ");
    Serial.println(wordNum-1);
    return;
  }
  digitalWrite(txEnPin, HIGH);
  Serial1.println();
  for (int i=0; i<30; i++) {
    Serial1.print(words[choice][i] + '1');
  }
  Serial1.println();
  Serial1.flush();
  digitalWrite(txEnPin, LOW);
}
