#include <Arduino.h>

constexpr uint8_t txEnPin = 2;

void setup() {
  Serial.begin(19200);
  Serial.setTimeout(INT32_MAX);
  pinMode(txEnPin, OUTPUT);
  //delay(1);
  digitalWrite(txEnPin, LOW);
  Serial1.begin(19200);

}

void loop() {
  // put your main code here, to run repeatedly:
  if (Serial.available() > 0) {
    char c = Serial.read();
    digitalWrite(txEnPin, HIGH);
    Serial1.write(c);
    Serial1.flush();
    digitalWrite(txEnPin, LOW);
  }
}