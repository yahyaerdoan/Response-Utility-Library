export abstract class Clipboard {
  abstract copy(text: string): Promise<boolean>;
}
