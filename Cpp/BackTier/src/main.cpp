#include <Arduino.h>
#include "CharLineComm.h"

constexpr int rows = 6;
constexpr int cols = 5;
constexpr uint8_t txEnPin = 2;

CharLineComm charLineComm(Serial1, 255);

const uint8_t matrixPins[cols][rows] = {
    {24, 26, 28, 23, 25, 27},
    {10,  9,  8,  7,  6, 22},
    {39, 41, 43, 13, 12, 11},
    {42, 44, 31, 33, 35, 37},
    {30, 32, 34, 36, 38, 40},
};

uint8_t screen[cols] = {0,0,0,0,0};
constexpr int switchNum = 4;
uint8_t switchPins[switchNum] = {A4, A5, A6, A7};

int boardNum = 0;

extern uint8_t *currentDisplay;
constexpr int bytesPerBoard = 5;

void pollComm(){}


void delayWithRefresh(unsigned long duration)
{
    unsigned long until = millis() + duration;
    while (millis() < until) {
        pollComm();
    }

}


inline bool getPixel(const uint8_t *bitmap, int x, int y)
{
    return (bitmap[x] & (0b00100000 >> y)) != 0;
}


void setPixel(int x, int y, bool color) {
    if (getPixel(screen, x, y) == color) return;
    uint8_t pin = matrixPins[x][y];
    pinMode(pin, OUTPUT);
    if (color) {
        digitalWrite(pin, LOW);
        screen[x] |= 0b00100000 >> y;
        delayWithRefresh(30);
        //Serial.println(int(pin));  
    } else {
        screen[x] &= ~(0b00100000 >> y);
        digitalWrite(pin, HIGH);
        delayWithRefresh(30);
    }
}

void setFrame(const uint8_t *frame)
{
    for (int y=0; y<rows; y++) {
        for (int x=0; x<cols; x++) {
            setPixel(x,y,getPixel(frame, x, y));
        }
    }
}

void setup()
{
    Serial.begin(9600);
    pinMode(txEnPin, OUTPUT);
    digitalWrite(txEnPin, LOW);
    Serial1.begin(19200);
    // put your setup code here, to run once:
    for (int i=0; i<switchNum; i++) {
        pinMode(switchPins[i], INPUT_PULLUP);
        boardNum <<= 1;
        boardNum |= digitalRead(switchPins[i]) == LOW ? 1 : 0;
    }
}

void loop()
{
    const char *generalBitmapAsString = charLineComm.getNextLine(); // no delays while processing
    uint8_t frame[cols];
    generalBitmapAsString += boardNum * cols;
    for (int i=0; i < cols; i++, generalBitmapAsString++) {
        frame[i] = uint8_t(*generalBitmapAsString - '1');
    }
    // processing ended, now we can doe delay with polls
    setFrame(frame);
}
