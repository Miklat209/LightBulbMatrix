#include "CharLineComm.h"
#include <HardwareSerial.h>

CharLineComm::CharLineComm(Stream &inStream, size_t maxLineSize) :
    _inStream(inStream)
{
    _maxLineSize = maxLineSize;
    _line[0] = new char[maxLineSize+1];
    _line[1] = new char[maxLineSize+1];
}

// d'tor won't really be called, but...

CharLineComm::~CharLineComm()
{
    delete [](_line[0]);
    delete [](_line[1]);
}

void CharLineComm::endCurrentLine()
{
    //Serial.println("endCurrentLine");
    _line[_curLineIdx][_curPos] = '\0';
    _curLineIdx = 1 - _curLineIdx;
    _curPos = 0;
    _hasPendingLine = true;

}

void CharLineComm::Poll()
{
//    Serial.println(_inStream.available());
    while (_inStream.available() > 0) {
//        Serial.println(" Got a char");
        if (_curPos < _maxLineSize) {
            _inStream.readBytes(&(_line[_curLineIdx][_curPos]),1);
            if (_line[_curLineIdx][_curPos] == '\r') continue;
            //Serial.print(_line[_curLineIdx][_curPos]);
            if (_line[_curLineIdx][_curPos] == '\n') {
                //Serial.println(_curPos);
                //Serial.print("In position 0 :");
                //Serial.println(int(_line[_curLineIdx][0]));
                if (_curPos == 0) { // empty line
                    _hasSeenEmptyLine = true;
                    //Serial.println("seen empty line");
                } else {
                    endCurrentLine();
                }
            } else { // not eol
                _curPos++;
            }
        } else { // overflow
            char c;
            _inStream.readBytes(&c,1);
            if (c=='\n') {
                endCurrentLine();
            }
        }
    }
}

const char *CharLineComm::getNextLine()
{
    while (!_hasSeenEmptyLine) { // in case of restart in the middle of communication
        Poll();
    }
    while (!_hasPendingLine) {
        Poll();
    }
    _hasPendingLine = false;
    return _line[1-_curLineIdx];
}
