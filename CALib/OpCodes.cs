using System;

namespace CALib
{
    public enum OpCodes
    {
        nop = 0,
        add = 1,
        sub = 2,
        mul = 3,
        div = 4,
        mod = 5,

        and,
        orx,
        not,
        xor,

        jmp,

        set,
        jiz,
        jnz,

        mov,

        equ,
        lth,
        leq,

        str,
        lod,

        psh,
        pop,

        pek,
        cal,
        ret,
        jmi,
        jzi,
        jni,

        sti,
        ldi,

        cli,
        shl,
        shr
    }
}
