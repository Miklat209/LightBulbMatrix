#ifndef __CharLineComm_h_
#define __CharLineComm_h_

#include <Stream.h>

class CharLineComm {
private:
    bool _hasSeenEmptyLine = false;
    bool _hasPendingLine = false;
    size_t _curPos = 0;
    char *(_line[2]);
    int _curLineIdx = 0;
    size_t _maxLineSize;
    Stream &_inStream;
    void endCurrentLine();
public:
    CharLineComm(Stream &inStream, size_t maxLineSize);
    ~CharLineComm();
    void Poll();
    const char *getNextLine();
};

#endif // __CharLineComm_h_