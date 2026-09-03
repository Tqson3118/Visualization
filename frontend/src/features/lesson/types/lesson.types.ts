export interface QuizQuestion {
  id: string;
  questionText: string;
  type?: 'SINGLE' | 'MULTIPLE' | 'MULTI' | string;
  options: string[];
  correctIndex?: number;
  correctIndices?: number[];
  explanation?: string;
}

export interface TestCase {
  input: string;
  expectedOutput: string;
  isHidden?: boolean;
}

export interface CodeLabTask {
  id?: string;
  title?: string;
  description: string;
  initialCode: string;
  solution: string;
  testCases: TestCase[];
  /** Tên hàm entry point được gọi với các tham số parse từ `TestCase.input` (mặc định: solution). */
  entryFunction?: string;
  /** Gợi ý phân tầng (tiered hints). */
  hints?: string[];
}

export interface Lesson {
  id: string;
  title: string;
  algorithmId: string;
  xpReward: number;
  theoryContent: string;
  quizQuestions?: QuizQuestion[];
  codelabTask?: CodeLabTask | CodeLabTask[];
  lastQuizSubmission?: {
    score: number;
    maxScore: number;
    passed: boolean;
    answersJson?: string | null;
    resultJson?: string | null;
    submittedAt?: string | null;
  } | null;
  lastSubmittedCode?: string | null;
}
