const int mutePin1 = 13;
const int volumePin1 =  0;
const int mutePin2 = 12;
const int volumePin2 =  1;

int muteState1 = 0;
int volumeVal1 = 0;

int muteState2 = 0;
int volumeVal2 = 0;

void setup() {
  // put your setup code here, to run once:
  pinMode(mutePin1, INPUT_PULLUP);
  pinMode(mutePin2, INPUT);
  Serial.begin(9600);
}

String leadingZeroes(int val) {
  if (val < 10) {
    return "00" + String(val);
  }
  else if (val < 100) {
    
    return "0" + String(val);
  }
  else {
    return String(val);
  }
}

void loop() {
  // put your main code here, to run repeatedly:
  muteState1 = digitalRead(mutePin1);
  volumeVal1 = analogRead(volumePin1);
  
  muteState2 = digitalRead(mutePin2);
  volumeVal2 = analogRead(volumePin2);

  // Channel 1
  Serial.print(muteState1);
  Serial.print("-");
  Serial.print(leadingZeroes(volumeVal1));
  Serial.print("-");
  // Channel 2
  Serial.print(muteState2);
  Serial.print("-");
  Serial.print(leadingZeroes(volumeVal2));
  Serial.println();
  
  delay(100);
}
